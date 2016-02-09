using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("URandomGen")]
[assembly: AssemblyDescription("A .Net library containing alternate implementations of the System.Random type.")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("URandomGen")]
[assembly: AssemblyCopyright("Copyright © 2015 by KimikoMuffin")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if ANDROID
[assembly: AssemblyConfiguration("For Android")]
#elif IOS
[assembly: AssemblyConfiguration("For iOS")]
#elif NET_2_0
[assembly: AssemblyConfiguration("For .Net 2.0")]
#elif NET_3_5
[assembly: AssemblyConfiguration("For .Net 3.5")]
#elif NET_4_0
[assembly: AssemblyConfiguration("For .Net 4.0")]
#elif NET_4_5
[assembly: AssemblyConfiguration("For .Net 4.5")]
#elif NET_4_6
[assembly: AssemblyConfiguration("For .Net 4.6")]
#elif PCL_4_5
[assembly: AssemblyConfiguration("For PCL 4.5")]
#elif PCL_4_0
[assembly: AssemblyConfiguration("For PCL 4.0")]
#endif

#if !NOCOM
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.8.2.0")]
[assembly: AssemblyFileVersion("0.8.2.0")]
