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

using Result = LacunaExpanse.Response.Result; //Example of namespace alias

namespace LacunaExpanse
{
	static class Archaeology
	{
		static string URL = "/archaeology";
		
		static Dictionary<string,Stack<string>> glyphs;
		
		static Archaeology ()
		{
			glyphs = new Dictionary<string,Stack<string>>();

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
        
    static string [] pType = new string[] {
      "p11","p12","p11","p12","p12","p11","p11","p12","p11","p12",
      "p12","p11","p12","p11","p12","p11","p12","p11","p12","p11"};
		
		public static void AssembleGlyphs (Session session, string buildingID, params string [] ids)
		{		
			JsonTextWriter req = session.Request("assemble_glyphs",
			                                     session.cache.session_id,
			                                     buildingID);
			req.WriteStartArray ();
			foreach (string id in ids)
				req.WriteString(id);
			
			session.Post(URL,req);
		}
		
		public static void BuildHalls (Session session, string buildingID, int save)
		{
			int count,total=0;
			
			foreach (string [] glyph in HallRecipe)
			{
				count = Math.Min(Math.Min(((Stack<string>) glyphs[glyph[0]]).Count,
		    	                      	((Stack<string>) glyphs[glyph[1]]).Count),
		      	           	 Math.Min(((Stack<string>) glyphs[glyph[2]]).Count,
		        	                  	((Stack<string>) glyphs[glyph[3]]).Count));
			
				for (int i = count; i > save; i--) {			
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
			
			Console.WriteLine("\n{0} hall(s) assembled!",total);

      IEnumerator pEnum = pType.GetEnumerator();

      foreach (string [] recipe in HallRecipe)
      {
        Console.WriteLine();
        foreach (string glyph in recipe)
        {
          pEnum.MoveNext();
          Console.WriteLine("{0,3:D} {1}-{2}",
                            ((Stack<string>) glyphs[glyph]).Count,
                            pEnum.Current.ToString(),glyph);
        }
      }
		}
		
		public static void GetGlyphs (Session session, string buildingID)
		{
			JsonTextWriter req = session.Request("get_glyphs",
			                                     session.cache.session_id,
			                                     buildingID);
			
			//session.onData += ProcessResult;
			
			if (session.Post(URL,req) == 0) {
        //Next two lines perform same thing old ProcessResult did
        foreach (Result.Glyph glyph in session.response.result.glyphs)
          ((Stack<string>) (glyphs[glyph.type])).Push(glyph.id);

        IEnumerator pEnum = pType.GetEnumerator();
				foreach (string [] recipe in HallRecipe)
				{
					Console.WriteLine();
					foreach (string glyph in recipe)
          {
            pEnum.MoveNext();
            Console.WriteLine("{0,3:D} {1}-{2}",
                            ((Stack<string>) glyphs[glyph]).Count,
                            pEnum.Current.ToString(),glyph);
          }
				}
				
				int count = 0;

        foreach (string [] glyph in HallRecipe)
					count += Math.Min(Math.Min(((Stack<string>) glyphs[glyph[0]]).Count,
				                           	((Stack<string>) glyphs[glyph[1]]).Count),
				                  	Math.Min(((Stack<string>) glyphs[glyph[2]]).Count,
				                           	((Stack<string>) glyphs[glyph[3]]).Count));

        Console.WriteLine("\nYou can build {0} hall(s)",count);
			}
			//session.onData -= ProcessResult;
		}

    // Code below is no longer being used (old Jayrock processing method)

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

