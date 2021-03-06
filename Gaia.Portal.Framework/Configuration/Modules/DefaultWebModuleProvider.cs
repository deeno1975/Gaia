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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Gaia.Core.Logging;
using Gaia.Portal.Framework.Bundling;
using Newtonsoft.Json;

namespace Gaia.Portal.Framework.Configuration.Modules
{
	public class DefaultWebModuleProvider : IWebModuleProvider
	{
		#region
		private const string MODULE_MASK = @"{Module}";
		private const string INCLUDE_FILE_MASK = @"{ModuleConfig}";

		private readonly IConfiguration _config;
		private readonly ILog _log;

		#endregion


		#region Public properties
		public IList<IWebModule> Modules { get; private set; }
		#endregion

		#region Constructors 

		public DefaultWebModuleProvider(IConfiguration configuration)
		{
			_config = configuration;
			_log = LogManager.GetLogger(GetType());

			if (Modules == null)
			{
				LoadModules();
			}
		}
		#endregion


		public void LoadModules()
		{
			Modules = new List<IWebModule>();

			var areasRootPath = _config.ApplicationSettings.ModulesRoot;
			var moduleScriptVirtualPath = _config.ApplicationSettings.ModuleIncludeMask.Replace(INCLUDE_FILE_MASK, "");
			var areasRootDir = new DirectoryInfo(HttpContext.Current.Server.MapPath(areasRootPath));

			if (areasRootDir.Exists)
			{
				foreach (var dir in areasRootDir.GetDirectories())
				{
					var includeVirtPath = _config.ApplicationSettings.ModuleIncludeMask.Replace(MODULE_MASK, dir.Name)
						.Replace(INCLUDE_FILE_MASK, _config.ApplicationSettings.ModuleConfigFile);
					var moduleConfigFile = new FileInfo(HttpContext.Current.Server.MapPath(includeVirtPath));

					if (moduleConfigFile.Exists)
					{
						try
						{
							var webModule =
								JsonConvert.DeserializeObject<DefaultWebModule>(File.ReadAllText(moduleConfigFile.FullName));

							if (!string.IsNullOrEmpty(webModule.IocContainerConfig))
							{
								var contFullPath =
									HttpContext.Current.Server.MapPath(
										$"{moduleScriptVirtualPath}{webModule.IocContainerConfig}".Replace(MODULE_MASK, dir.Name));
								if (File.Exists(contFullPath))
								{
									webModule.IocContainerFullPath = contFullPath;
								}
							}

							var modulesScriptBundle = new ScriptBundle(_config.ApplicationSettings.ModuleScriptBundleMask.Replace(MODULE_MASK, dir.Name))
							{
								Orderer = new NonOrderingBundleOrderer()
							};

							foreach (var p in webModule.Scripts.Select(p => $"{moduleScriptVirtualPath}{p}".Replace(MODULE_MASK, dir.Name)))
							{
								try
								{
									modulesScriptBundle.Include(p);
								}
								catch (ArgumentException e)
								{
									if (e.ParamName == "directoryVirtualPath")
									{
										_log.ErrorFormat("Directory for virtual path '{0}' doesn't exists", p);
									}
									else
									{
										throw;
									}
								}
							}

							webModule.Bundles.Add(modulesScriptBundle);
							Modules.Add(webModule);
						} catch (Exception e)
						{
							_log.Error(e, "Error during web modules loading");
							throw new Core.Exceptions.GaiaBaseException("Error during web modules loading", e);
						}
					}
					else
					{
						_log.ErrorFormat("Module configuration file doesn't exists. File '{0}'", moduleConfigFile.FullName);
					}
				}
			}

		}
	}
}
