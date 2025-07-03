// Because we have to use Windows.Forms for the ColorDialog, we have to add "<UseWindowsForms>true</UseWindowsForms>"
// to the project file. Without any further changes, this will generate GlobalUsings in the obj folder, which includes
// Windows Forms namespaces. This in turn causes conflicts with Color, Font and other types in ImageSharp.
//
// To avoid these conflicts, we disable "<ImplicitUsings>" in the project file and define our own global using.
// The following lines are the ones of a WPF project with WindowsForms enabled.

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;