//
// Copyright (c) 2008-2015 the Urho3D project.
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

namespace Urho.Samples
{
	/// <summary>
	/// A set of predefined Joystick layouts (mobile only)
	/// </summary>
	public static class JoystickLayoutPatches
	{
		public const string WithZoomInAndOut =
			"<patch>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Zoom In</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"PAGEUP\" />" +
			"        </element>" +
			"    </add>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Zoom Out</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"PAGEDOWN\" />" +
			"        </element>" +
			"    </add>" +
			"</patch>";

		public const string WithZoomInAndOutWithoutArrows =
			"<patch>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button2']]\">" +
			"        <attribute name=\"Is Visible\" value=\"false\" />" +
			"    </add>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Hat0']]\">" +
			"        <attribute name=\"Is Visible\" value=\"false\" />" +
			"    </add>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Up</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"UP\" />" +
			"        </element>" +
			"    </add>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Down</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"DOWN\" />" +
			"        </element>" +
			"    </add>" +
			"</patch>";

		public const string Hidden =
			"<patch>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Hat0']]\">" +
			"        <attribute name=\"Is Visible\" value=\"false\" />" +
			"    </add>" +
			"</patch>";

		public const string WithDebugButton =
			"<patch>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Debug</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"SPACE\" />" +
			"        </element>" +
			"    </add>" +
			"</patch>";

		public const string WithFireAndDebugButtons =
			"<patch>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Fire</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"MouseButtonBinding\" />" +
			"            <attribute name=\"Text\" value=\"LEFT\" />" +
			"        </element>" +
			"    </add>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Debug</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"SPACE\" />" +
			"        </element>" +
			"    </add>" +
			"</patch>";

		public const string WithGroupAndAnimationButtons =
			"<patch>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Group</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"G\" />" +
			"        </element>" +
			"    </add>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Animation</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"SPACE\" />" +
			"        </element>" +
			"    </add>" +
			"</patch>";
	}
}
