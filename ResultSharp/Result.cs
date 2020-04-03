﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Text;

namespace ResultSharp
{
	internal enum ResultState
	{
		Ok,
		Err,
	}

	[Serializable]
	public struct Result<T, E> : ISerializable
		where T : notnull
		where E : notnull
	{
		readonly ResultState State;
		readonly T Value;
		readonly E Error;

		Result(T value)
		{
			State = ResultState.Ok;
			Value = value;
			Error = default;
		}

		Result(E error)
		{
			State = ResultState.Err;
			Value = default;
			Error = error;
		}

		Result(
			SerializationInfo info,
			StreamingContext context)
		{
			State = (ResultState)info.GetValue(nameof(State), typeof(ResultState));
			switch (State)
			{
				case ResultState.Ok:
					Value = (T)info.GetValue(nameof(Value), typeof(T));
					Error = default;
					break;

				case ResultState.Err:
					Value = default;
					Error = (E)info.GetValue(nameof(Error), typeof(E));
					break;

				default:
					throw new Exception($"Unexpected state: {State}");
			}
		}

		[Pure]
		public void GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
			info.AddValue(nameof(State), State);
			switch (State)
			{
				case ResultState.Ok:
					info.AddValue(nameof(Value), Value);
					break;

				case ResultState.Err:
					info.AddValue(nameof(Error), Error);
					break;
			}
		}

		[Pure]
		public bool IsOk =>
			State == ResultState.Ok;

		[Pure]
		public bool IsErr =>
			State == ResultState.Err;

		[Pure]
		public Ret Match<Ret>(Func<T, Ret> ok, Func<E, Ret> err) =>
			IsOk
				? ok(Value)
				: err(Error);

		[Pure]
		public Result<U, E> Map<U>(Func<T, U> op) where U : notnull =>
			Match<Result<U, E>>(val => op(val), err => err);

		[Pure]
		public U MapOr<U>(Func<T, U> op, U defaultValue) where U : notnull =>
			Match(op, _ => defaultValue);

		[Pure]
		public U MapOrElse<U>(Func<T, U> op, Func<E, U> elseOp) where U : notnull =>
			Match(op, elseOp);

		[Pure]
		public Result<T, F> MapErr<F>(Func<E, F> op) where F : notnull =>
			Match<Result<T, F>>(val => val, err => op(err));

		[Pure]
		public Result<U, E> And<U>(Result<U, E> result) where U : notnull =>
			Match(_ => result, err => err);

		[Pure]
		public Result<U, E> AndThen<U>(Func<T, Result<U, E>> op) where U : notnull =>
			Match(val => op(val), err => err);

		[Pure]
		public Result<T, F> Or<F>(Result<T, F> result) where F : notnull =>
			Match(val => val, _ => result);

		[Pure]
		public Result<T, F> OrElse<F>(Func<E, Result<T, F>> op) where F : notnull =>
			Match(val => val, err => op(err));

		[Pure]
		public T Unwrap() =>
			Match(
				val => val,
				_ => throw new UnwrapException(Messages.UnwrapCalledOnAFaultedResult));

		[Pure]
		public T UnwrapOr(T defaultValue) =>
			Match(val => val, _ => defaultValue);

		[Pure]
		public T UnwrapOrElse(Func<T> op) =>
			Match(val => val, _ => op());

		[Pure]
		public E UnwrapErr() =>
			Match(
				_ => throw new UnwrapException(Messages.UnwrapErrCalledOnAnOkResult),
				err => err);

		[Pure]
		public T Expect(string msg) =>
			Match(
				val => val,
				_ => throw new ExpectException(msg));

		[Pure]
		public E ExpectErr(string msg) =>
			Match(
				_ => throw new ExpectException(msg),
				err => err);

		[Pure]
		public override string ToString() =>
			Match(
				val => $"Ok({val})",
				err => $"Err({err})");

		[Pure]
		public static Result<T, E> Ok(T value) =>
			new Result<T, E>(value);

		[Pure]
		public static Result<T, E> Err(E error) =>
			new Result<T, E>(error);

		[Pure]
		public static implicit operator Result<T, E>(T value) =>
			Ok(value);

		[Pure]
		public static implicit operator Result<T, E>(E error) =>
			Err(error);

		[Pure]
		public static implicit operator Result<T, E>(ResultOk<T> ok) =>
			Ok(ok.Value);

		[Pure]
		public static implicit operator Result<T, E>(ResultErr<E> err) =>
			Err(err.Error);

		[Pure]
		public bool Equals(Result<T, E> other) =>
			other.Match(Equals, Equals);

		[Pure]
		public bool Equals(T other) =>
			Match(val => val.Equals(other), _ => false);

		[Pure]
		public bool Equals(E other) =>
			Match(_ => false, err => err.Equals(other));

		[Pure]
		public static bool operator ==(Result<T, E> a, Result<T, E> b) =>
			a.Equals(b);

		[Pure]
		public static bool operator !=(Result<T, E> a, Result<T, E> b) =>
			!(a == b);

		[Pure]
		public override bool Equals(object obj) =>
			obj switch
			{
				Result<T, E> x => Equals(x),
				ResultOk<T> x => Equals(x),
				ResultErr<E> x => Equals(x),
				T x => Equals(x),
				E x => Equals(x),
				_ => false,
			};

		[Pure]
		public override int GetHashCode() =>
			Match(
				val => Tuple.Create(ResultState.Ok, val).GetHashCode(),
				err => Tuple.Create(ResultState.Err, err).GetHashCode());
	}

	public struct ResultOk<T> where T : notnull
	{
		internal readonly T Value;
		internal ResultOk(T value) =>
			Value = value;
	}

	public struct ResultErr<E> where E : notnull
	{
		internal readonly E Error;
		internal ResultErr(E error) =>
			Error = error;
	}
}
