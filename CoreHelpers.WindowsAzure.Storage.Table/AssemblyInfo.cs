﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompany("Core Helpers")]
[assembly: AssemblyProduct("WindowsAzure.Storage.Table")]
[assembly: AssemblyTitle("CoreHelpers Azure Storage Abstraction")]

[assembly: AssemblyFileVersion("2.0.0.0")]
[assembly: AssemblyVersion("2.0.0.0")]

#if (DEBUG)
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: InternalsVisibleTo("CoreHelpers.WindowsAzure.Storage.Table.Tests")]