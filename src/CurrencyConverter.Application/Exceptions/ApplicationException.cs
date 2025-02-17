﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Exceptions
{
	public class ApplicationException :Exception
	{
		public ApplicationException(string message) : base(message)
		{
		}

		public ApplicationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
