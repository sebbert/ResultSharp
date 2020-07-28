﻿using System;

namespace ResultSharp
{
	public static class Prelude
	{
		public static ResultOk<Unit> Ok() =>
			new ResultOk<Unit>(Unit.Default);

		public static ResultOk<T> Ok<T>(T value) =>
			new ResultOk<T>(value);

		public static ResultErr<E> Err<E>(E error) =>
			new ResultErr<E>(error);

		public static Result<T, E> OkIf<T, E>(bool condition, T value, E error) =>
			condition
				? Result<T, E>.Ok(value)
				: Result<T, E>.Err(error);

		public static Result<T, E> OkIf<T, E>(bool condition, Func<T> getValue, Func<E> getError) =>
			condition
				? Result<T,E>.Ok(getValue())
				: Result<T,E>.Err(getError());

		public static Result OkIf(
			bool condition,
			string error) =>
				OkIf(condition, new Unit(), error);

		public static Result<T, E> ErrIf<T, E>(
			bool condition,
			T value,
			E error) =>
				OkIf(!condition, value, error);

		public static Result<T, E> ErrIf<T, E>(
			bool condition,
			Func<T> getValue,
			Func<E> getError) =>
				OkIf(!condition, getValue, getError);

		public static Result ErrIf(
			bool condition,
			string error) =>
			ErrIf(condition, new Unit(), error);

		public static Result<T, Exception> Try<T>(Func<T> op) =>
			Try<T, Exception>(op);

		public static Result<T, E> Try<T, E>(Func<T> op)
			where E : Exception
		{
			try
			{
				return Ok(op());
			}
			catch (E exception)
			{
				return Err(exception);
			}
		}
	}
}
