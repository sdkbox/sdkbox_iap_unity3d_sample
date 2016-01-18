/*****************************************************************************
Copyright © 2015 SDKBOX.

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
*****************************************************************************/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using sdkbox;

namespace sdkbox
{
	public class MyWindow : EditorWindow
	{
		string myString = "Hello World";
		bool groupEnabled;
		bool myBool = true;
		float myFloat = 1.23f;
		
		// Add menu item named "My Window" to the Window menu
		[MenuItem("Window/My Window")]
		public static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(MyWindow));
		}
		
		void OnGUI()
		{
			GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
			myString = EditorGUILayout.TextField ("Text Field", myString);
			
			groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
			myBool = EditorGUILayout.Toggle ("Toggle", myBool);
			myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
			EditorGUILayout.EndToggleGroup ();
		}
	}

	public class utils 
	{
		// returns true if there is a AndroidManifest.xml 
		// file at the desired location, false otherwise.
		public static bool hasAndroidManifest()
		{
			return File.Exists("Assets/Plugins/Android/AndroidManifest.xml");
		}

		// Execute an SDKBOX command and return the output.
		public static string runSdkboxCommand(string arguments)
		{
			Process p = new Process();
			p.StartInfo.FileName = "python";
			p.StartInfo.Arguments = "sdkbox.pyc " + arguments;    
		
			p.StartInfo.RedirectStandardError=true;
			p.StartInfo.RedirectStandardOutput=true;
			p.StartInfo.CreateNoWindow = true;
			
			p.StartInfo.WorkingDirectory = Application.dataPath; 
			p.StartInfo.UseShellExecute = false;
			
			p.Start();
			string output = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
			p.Close();
			
			return output;
		}
	}
}
