// 
//  Archaeology.cs
//  
//  Author:
//       brian <${AuthorEmail}>
//  
//  Copyright (c) 2012 brian
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace LacunaExpanse
{
	static class Archaeology
	{
		static string URL = "/archaeology";
		
		static Dictionary<string,object> glyphs;
		
		static Archaeology ()
		{
			glyphs = new Dictionary<string, object>();
			
			// Build the glyph dictionary!
			foreach (string [] recipe in HallRecipe)
				foreach (string glyph in recipe)			
					glyphs.Add(glyph,new Stack<string>());
		}
				
		static string [][] HallRecipe = new string[][] {
			new string [] { "goethite","halite","gypsum","trona"},
			new string [] { "gold","anthracite","uraninite","bauxite"},
			new string [] { "kerogen","methane","sulfur","zircon"},
			new string [] { "rutile","chromite","chalcopyrite","galena"},
			new string [] { "monazite","fluorite","beryl","magnetite"}};			
		
		public static void BuildHalls (Session session, string buildingID)
		{
			int count,total=0;
			
			foreach (string [] glyph in HallRecipe)
			{
				count = Math.Min(Math.Min(((Stack<string>) glyphs[glyph[0]]).Count,
		    	                      	((Stack<string>) glyphs[glyph[1]]).Count),
		      	           	 Math.Min(((Stack<string>) glyphs[glyph[2]]).Count,
		        	                  	((Stack<string>) glyphs[glyph[3]]).Count));
			
				for (int i = count; i > 0; i--) {			
					JsonTextWriter req = session.Request("assemble_glyphs",
				                                     session.cache.session_id,
				                                     buildingID);
					req.WriteStartArray ();
				
					for (int j=0; j < glyph.Length; j++)
						req.WriteString(((Stack<string>) glyphs[glyph[j]]).Pop());
					
					if (session.Post(URL,req) != 0) break;
					
					total++;
				}
			}
			
			Console.WriteLine("\n{0} hall(s) assembled!\n",total);
			
			foreach (string [] recipe in HallRecipe)
			{
				foreach (string glyph in recipe)
					Console.WriteLine("{0} : {1}",glyph,((Stack<string>) glyphs[glyph]).Count);
				Console.WriteLine();
			}
		}
		
		public static void GetGlyphs (Session session, string buildingID)
		{
			JsonTextWriter req = session.Request("get_glyphs",
			                                     session.cache.session_id,
			                                     buildingID);
			
			session.onData += ProcessResult;
			session.Post(URL,req);
			session.onData -= ProcessResult;
			
			foreach (string [] recipe in HallRecipe)
			{
				foreach (string glyph in recipe)
					Console.WriteLine("{0} : {1}",glyph,((Stack<string>) glyphs[glyph]).Count);
				Console.WriteLine();
			}
			
			int count = 0;
			
			foreach (string [] glyph in HallRecipe)
				count += Math.Min(Math.Min(((Stack<string>) glyphs[glyph[0]]).Count,
			                           	((Stack<string>) glyphs[glyph[1]]).Count),
			                  	Math.Min(((Stack<string>) glyphs[glyph[2]]).Count,
			                           	((Stack<string>) glyphs[glyph[3]]).Count));
			
			Console.WriteLine("You can build {0} hall(s)",count);
		}
		
		static string myGlyph = "";
		
		static void ProcessResult (string result, string member, string data)
		{
			if (result != "glyphs") return;
			
			switch (member)
			{
				case "type":
					if (glyphs.ContainsKey(data)) {
						myGlyph = data;
					} else {
						glyphs.Add(data,new Stack<string>());
						myGlyph = data;
					}
					break;
				case "id":
					((Stack<string>) (glyphs[myGlyph])).Push(data);
					break;
				default:
					break;
			}
		}
	}
}

