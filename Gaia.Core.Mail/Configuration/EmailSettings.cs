﻿/*
The MIT License (MIT)

Copyright (c) 2012 Stepan Fryd (stepan.fryd@gmail.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Gaia.Core.Mail.Configuration
{
	[XmlRoot("emailSettings")]
	public class EmailSettings : IEmailSettings
	{
		#region Fields and constants

		private string _copyLocation;

		#endregion

		#region Public members

		[XmlAttribute("enabled")]
		public bool Enabled { get; set; }

		[XmlAttribute("testRecipient")]
		public string TestRecipient { get; set; }

		[XmlAttribute("testMode")]
		public bool TestMode { get; set; }

		[XmlAttribute("saveCopy")]
		public bool SaveCopy { get; set; }

		[XmlAttribute("copyLocation")]
		public string CopyLocation
		{
			get { return _copyLocation; }
			set
			{
				var path = value;
				if (value.StartsWith("~/") && HttpContext.Current != null)
				{
					path = HttpContext.Current.Server.MapPath(value);
				}

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				_copyLocation = path;
			}
		}

		#endregion
	}
}