﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;

using Microsoft.Test.Apex;
using Microsoft.Test.Apex.VisualStudio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Omni.Logging;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.VS
{
    [TestClass] // AssemblyInitialize won't be found without it
    public abstract class TestBase : VisualStudioHostTest
    {
        private static string _hiveName;

        protected TestBase()
        {
            // TestCleanup will fire up another instance of Visual Studio to reset 
            // the AutoloadExternalChanges if it thinks the default changed even if
            // that was just caused by settings to be sync'd. Just turn this feature off.
            SuppressReloadPrompt = false;
        }

        protected override bool IncludeReferencedAssembliesInHostComposition => false; // Do not add things we reference to the MEF Container

        protected override VisualStudioHostConfiguration GetHostConfiguration()
        {
            return new ProjectSystemHostConfiguration(_hiveName);
        }

        protected override OperationsConfiguration GetOperationsConfiguration()
        {
            return new ProjectSystemOperationsConfiguration(TestContext);
        }

        [TestInitialize]
        public override void TestInitialize()
        {
            // Make the omni log test specific and prevent incremental logging
            Log.ResetLog($"{GetType().Name}.{TestContext.TestName}");

            base.TestInitialize();
        }

        protected override void DoHostTestCleanup()
        {
            base.DoHostTestCleanup();

            TryShutdownVisualStudioInstance();
        }

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            _hiveName = GetVsHiveName(context);

            SetTestInstallationDirectoryIfUnset();
        }

        private static string GetVsHiveName(TestContext context)
        {
            string rootSuffix = (string)context.Properties["VsRootSuffix"];
            if (!string.IsNullOrEmpty(rootSuffix))
                return rootSuffix;

            return Environment.GetEnvironmentVariable("RootSuffix") ?? "Exp";
        }

        private static void SetTestInstallationDirectoryIfUnset()
        {
            string installationUnderTest = "VisualStudio.InstallationUnderTest.Path";

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(installationUnderTest)))
            {
                string vsDirectory = Environment.GetEnvironmentVariable("VSAPPIDDIR");
                string devenv = Environment.GetEnvironmentVariable("VSAPPIDNAME");
                if (!string.IsNullOrEmpty(vsDirectory) && !string.IsNullOrEmpty(devenv))
                {
                    Environment.SetEnvironmentVariable(installationUnderTest, Path.Combine(vsDirectory, devenv));
                }
            }
        }
    }
}
