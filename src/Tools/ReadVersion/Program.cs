//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2022, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/03/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using static VersionCommon.ConsoleHelpers;

namespace ReadVersion
{
    public class Program
    {
        public static int Main(string[] args)
        {
            ArgNames = new[] { "ProjectFileOrPath" };
            ArgExamples = new[] { @"""D:\Projects\gemstone\io""" };

            try
            {
                if (!ValidateArgs(args))
                    return ExitBadArgs;

                string projectFileSearchPath = args[0].Trim();
                List<string> projectFilePaths = new();
                
                if (!ValidateProjectPath(projectFilePaths, projectFileSearchPath, out int result))
                    return result;

                foreach (string projectFilePath in projectFilePaths)
                {
                    // Load XML project file
                    XmlDocument projectFile = OpenProjectFile(projectFilePath);

                    // Get version number
                    if (!TryGetVersionNode(projectFile, out XmlNode versionNode))
                        continue;

                    TryGetCopyrightNode(projectFile, out XmlNode copyrightNode);

                    // Get version information without any suffix, e.g., remove any -beta suffix
                    string version = GetRawVersion(versionNode.InnerText);

                    // Get copyright, if any
                    string copyright = copyrightNode?.InnerText ?? string.Empty;

                    // Write proxy AssemblyInfo class
                    string path = Path.Combine(Path.GetDirectoryName(projectFilePath) ?? string.Empty, "AssemblyInfo.cs");

                    AssemblyInfo assemblyInfo = new()
                    {
                        Version = version,
                        Copyright = copyright
                    };

                    File.WriteAllText(path, assemblyInfo.TransformText(), Encoding.UTF8);

                    return ExitSuccess;
                }

                ShowError("No <Version> tag found.");
                return ExitNoVersion;
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
