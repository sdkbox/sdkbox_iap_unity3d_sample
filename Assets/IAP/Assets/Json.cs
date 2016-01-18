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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace sdkbox
{
	public class Json 
	{
		public enum Type {NUL, NUMBER, BOOL, STRING, ARRAY, OBJECT};

		private Type   _type;
		private double _d;
		private bool   _b;
		private string _s;

		private List<Json> _a;
		private Dictionary<string, Json> _o;

		public static Json null_json;

		public Json()
		{
			_type = Type.NUL;
		}

		public Json(bool b)
		{
			_type = Type.BOOL;
			_b = b;
		}

		public Json(double d)
		{
			_type = Type.NUMBER;
			_d = d;
		}

		public Json(string s)
		{
			_type = Type.STRING;
			_s = s;
		}

		public Json(List<Json> a)
		{
			_type = Type.ARRAY;
			_a = a;
		}

		public Json(Dictionary<string, Json> o)
		{
			_type = Type.OBJECT;
			_o = o;
		}

		public Type type()
		{
			return _type;
		}

		public bool is_null()
		{
			return type() == Type.NUL;
		}

		public bool is_valid()
		{
			return type() != Type.NUL;
		}

		// @brief return the integer value from the Json value.
		// if this value is not type NUMBER then the result is undefined.
		public int int_value()
		{
			return (int)_d;
		}

		public bool bool_value()
		{
			return _b;
		}

		// @brief return the float value from the Json value.
		// if this value is not type NUMBER then the result is undefined.
		public float float_value()
		{
			return (float)_d;
		}

		// @brief return the double value from the Json value.
		// if this value is not type NUMBER then the result is undefined.
		public double double_value()
		{
			return _d;
		}

		// @brief return the string value from the Json value.
		// if this value is not type STRING then the result is undefined.
		public string string_value()
		{
			return _s;
		}

		// @brief return the array value from the Json value.
		// if this value is not type ARRAY then the result is undefined.
		public List<Json> array_items()
		{
			return _a;
		}
		
		// @brief return the object value from the Json value.
		// if this value is not type OBJECT then the result is undefined.
		public Dictionary<string, Json> object_items()
		{
			return _o;
		}

		// @brief operator to support json[index]
		public Json this[int index]
		{
			get { return _a[index];  }
			set { _a[index] = value; }
		}

		// @brief operator to suport json["key"]
		public Json this[string key]
		{
			get { return _o[key];  }
			set { _o[key] = value; }
		}

		public static Json parse(string jsonString)
		{
			JsonParser parser = new JsonParser(jsonString);
			Json result = parser.parse_json(0);
			
			// Check for any trailing garbage
			parser.consume_whitespace();
			
			if (parser.i != jsonString.Length)
			{
				char c = jsonString[parser.i];
				return new Json(parser.fail("unexpected trailing " + parser.esc(c)));
			}

			return result;
		}

		private string encode(string value) 
		{
			string o = "\"";
			for (var i = 0; i < value.Length; i++) {
				char ch = value[i];
				if (ch == '\\') {
					o += "\\\\";
				} else if (ch == '"') {
					o += "\\\"";
				} else if (ch == '\b') {
					o += "\\b";
				} else if (ch == '\f') {
					o += "\\f";
				} else if (ch == '\n') {
					o += "\\n";
				} else if (ch == '\r') {
					o += "\\r";
				} else if (ch == '\t') {
					o += "\\t";
				} else if (ch <= 0x1f) {
					o += string.Format("\\u%{0:x4}", ch);
				} else if (ch == 0xe2 && value[i+1] == 0x80 && value[i+2] == 0xa8) {
					o += "\\u2028";
					i += 2;
				} else if (ch == 0xe2 && value[i+1] == 0x80 && value[i+2] == 0xa9) {
					o += "\\u2029";
					i += 2;
				} else {
					o += ch;
				}
			}
			o += "\"";
			return o;
		}

		public string dump()
		{
			switch (type())
			{
			case Type.NUL:
				return "nul";
			case Type.NUMBER:
				return string.Format("{0}", _d);
			case Type.BOOL:
				return _b ? "\"true\"" : "\"false\"";
			case Type.STRING:
				return encode(_s);
			case Type.ARRAY:
			{
				string s = "[";
				foreach (var j in _a) 
				{
					s += j.dump() + ",";
				}
				int l = s.Length;
				if (s[l-1] == ',')
					s = s.Substring(0, l-1);
				s += ']';
				return s;
			}
			case Type.OBJECT:
			{
				string s = "{";
				foreach (var kvp in _o)
				{ 
					s += '\"' + kvp.Key + "\":" + kvp.Value.dump() + ",";
				}
				int l = s.Length;
				if (s[l-1] == ',')
					s = s.Substring(0, l-1);
				s += '}';
				return s;
			}
			default:
				return ""; // not use these for now
			}
		}

		class JsonParser
		{
			private int MAX_DEPTH  = 100;
			private int MAX_DIGITS = 15;
			private string str;
			private bool failed;
			
			public int i;
			public string err;
			
			public JsonParser(string jsonString)
			{
				i = 0;
				str = jsonString;
				err = "";
				failed = false;
			}

			public string fail(string msg)
			{
				if (!failed)
					err = msg;
				failed = true;
				return msg;
			}

			public T fail<T>(string msg, T err_ret)
			{
				if (!failed)
					err = msg;
				failed = true;
				return err_ret;
			}
			
			public string esc(char c)
			{
				if (c >= 0x20 && c <= 0x7f)
				{
					return string.Format("'{0}' ({1})", c, c);
				}
				else
				{
					return string.Format("({1})", c);
				}
			}
			
			public bool in_range(long x, long lower, long upper)
			{
				return (x >= lower && x <= upper);
			}
			
			public void consume_whitespace()
			{
				while (i < str.Length && (str[i] == ' ' || str[i] == '\r' || str[i] == '\n' || str[i] == '\t'))
					i++;
			}
			
			public char get_next_token()
			{
				consume_whitespace();
				if (i == str.Length)
					return fail("unexpected end of input", '\0');
				
				return str[i++];
			}
			
			public void encode_utf8(long pt, string o)
			{
				if (pt < 0)
					return;
				
				if (pt < 0x80) {
					o += pt;
				} else if (pt < 0x800) {
					o += (pt >> 6) | 0xC0;
					o += (pt & 0x3F) | 0x80;
				} else if (pt < 0x10000) {
					o += (pt >> 12) | 0xE0;
					o += ((pt >> 6) & 0x3F) | 0x80;
					o += (pt & 0x3F) | 0x80;
				} else {
					o += (pt >> 18) | 0xF0;
					o += ((pt >> 12) & 0x3F) | 0x80;
					o += ((pt >> 6) & 0x3F) | 0x80;
					o += (pt & 0x3F) | 0x80;
				}
			}
			
			public string parse_string()
			{
				string o = "";
				long last_escaped_codepoint = -1;
				
				while (true)
				{
					if (i == str.Length)
						return fail("unexpected end of input in string");
					
					char ch = str[i++];
					
					if (ch == '"')
					{
						encode_utf8(last_escaped_codepoint, o);
						return o;
					}
					
					if (in_range(ch, 0, 0x1f))
					{
						string s = "unescaped " + esc(ch) + " in string";
						return fail(s);
					}

					// The usual case: non-escaped characters
					if (ch != '\\')
					{
						encode_utf8(last_escaped_codepoint, o);
						last_escaped_codepoint = -1;
						o += ch;
						continue;
					}
					
					// Handle escapes
					if (i == str.Length)
						return fail("unexpected end of input in string");
					
					ch = str[i++];
					
					if (ch == 'u')
					{
						// Extract 4-byte escape sequence
						string esc = str.Substring(i, 4);
						for (int j = 0; j < 4; j++)
						{
							if (!in_range(esc[j], 'a', 'f') && !in_range(esc[j], 'A', 'F')
							    && !in_range(esc[j], '0', '9'))
								return fail("bad \\u escape: " + esc);
						}
						
						long codepoint = Convert.ToInt64(esc, 16);

						// JSON specifies that characters outside the BMP shall be encoded as a pair
						// of 4-hex-digit \u escapes encoding their surrogate pair components. Check
						// whether we're in the middle of such a beast: the previous codepoint was an
						// escaped lead (high) surrogate, and this is a trail (low) surrogate.
						if (in_range(last_escaped_codepoint, 0xD800, 0xDBFF)
						    && in_range(codepoint, 0xDC00, 0xDFFF)) {
							// Reassemble the two surrogate pairs into one astral-plane character, per
							// the UTF-16 algorithm.
							encode_utf8((((last_escaped_codepoint - 0xD800) << 10)
							             | (codepoint - 0xDC00)) + 0x10000, o);
							last_escaped_codepoint = -1;
						} else {
							encode_utf8(last_escaped_codepoint, o);
							last_escaped_codepoint = codepoint;
						}
						
						i += 4;
						continue;
					}
					
					encode_utf8(last_escaped_codepoint, o);
					last_escaped_codepoint = -1;
					
					if (ch == 'b') {
						o += '\b';
					} else if (ch == 'f') {
						o += '\f';
					} else if (ch == 'n') {
						o += '\n';
					} else if (ch == 'r') {
						o += '\r';
					} else if (ch == 't') {
						o += '\t';
					} else if (ch == '"' || ch == '\\' || ch == '/') {
						o += ch;
					} else {
						return fail("invalid escape character " + esc(ch));
					}
				}
			}
			
			public Json parse_number()
			{
				int start_pos = i;
				
				if (str[i] == '-')
					i++;
				
				// Integer part
				if (str[i] == '0')
				{
					i++;
					if (in_range(str[i], '0', '9'))
						return new Json(fail("leading 0s not permitted in numbers"));
				}
				else if (in_range(str[i], '1', '9'))
				{
					i++;
					while (in_range(str[i], '0', '9'))
						i++;
				}
				else
				{
					return new Json(fail("invalid " + esc(str[i]) + " in number"));
				}
				
				if (str[i] != '.' && str[i] != 'e' && str[i] != 'E' && (i - start_pos) <= MAX_DIGITS)
				{
					return new Json((double)int.Parse(str.Substring(start_pos)));
				}
				
				// Decimal part
				if (str[i] == '.')
				{
					i++;
					if (!in_range(str[i], '0', '9'))
						return new Json(fail("at least one digit required in fractional part"));
					
					while (in_range(str[i], '0', '9'))
						i++;
				}
				
				// Exponent part
				if (str[i] == 'e' || str[i] == 'E')
				{
					i++;
					
					if (str[i] == '+' || str[i] == '-')
						i++;
					
					if (!in_range(str[i], '0', '9'))
						return new Json(fail("at least one digit required in exponent"));
					
					while (in_range(str[i], '0', '9'))
						i++;
				}
				
				string fstr = str.Substring(start_pos, i - start_pos);
				return new Json(float.Parse(fstr));
			}
			
			public Json expect(string expected, Json res)
			{
				i--;
				if (str.CompareTo(expected) == 0)
				{
					i += expected.Length;
					return res;
				}
				else
				{
					return new Json(fail("parse error: expected " + expected + ", got " + str.Substring(i, expected.Length)));
				}
			}
			
			public Json parse_json(int depth)
			{
				if (depth > MAX_DEPTH)
				{
					return new Json(fail("exceeded maximum nesting depth"));
				}
				
				char ch = get_next_token();
				if (failed)
					return new Json();
				
				if (ch == '-' || (ch >= '0' && ch <= '9'))
				{
					i--;
					return parse_number();
				}
				
				if (ch == 't')
					return expect("true", new Json(true));
				
				if (ch == 'f')
					return expect("false", new Json(false));
				
				if (ch == 'n')
					return expect("null", new Json());
				
				if (ch == '"')
					return new Json(parse_string());
				
				if (ch == '{')
				{
					Dictionary<string, Json> data = new Dictionary<string, Json>();
					ch = get_next_token();
					if (ch == '}')
						return new Json(data);
					
					while (true)
					{
						if (ch != '"')
							return new Json(fail("expected '\"' in object, got " + esc(ch)));
						
						string key = parse_string();
						if (failed)
							return new Json();
						
						ch = get_next_token();
						if (ch != ':')
							return new Json(fail("expected ':' in object, got " + esc(ch)));
						
						data[key] = parse_json(depth + 1);
						if (failed)
							return new Json();
						
						ch = get_next_token();
						if (ch == '}')
							break;
						if (ch != ',')
							return new Json(fail("expected ',' in object, got " + esc(ch)));
						
						ch = get_next_token();
					}
					return new Json(data);
				}
				
				if (ch == '[')
				{
					List<Json> data = new List<Json>();
					ch = get_next_token();
					if (ch == ']')
						return new Json(data);
					
					while (true)
					{
						i--;
						data.Add(parse_json(depth + 1));
						if (failed)
							return new Json();
						
						ch = get_next_token();
						if (ch == ']')
							break;
						if (ch != ',')
							return new Json(fail("expected ',' in list, got " + esc(ch)));
						
						get_next_token();
					}
					return new Json(data);
				}
				
				return new Json(fail("expected value, got " + esc(ch)));
			}
		};
	}
}
