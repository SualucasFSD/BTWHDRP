using System;
using UnityEngine;
using System.Collections.Generic;
//using System.Diagnostics;
public class SheetSplit
{
    public static Dictionary<Language, Dictionary<string, string>> LoadCsv(string sheet, string Source)
    {
        Dictionary<Language, Dictionary<string, string>> codex = new Dictionary<Language, Dictionary<string, string>>();
        bool firstLinde = true;
        int idColum = 0;
        Dictionary<int,Language> langColumn= new Dictionary<int,Language>();
        string[] rows = sheet.Split(new[] { '\n', '\r' },System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string[] cell = row.Split(',');
            if(firstLinde)
            {
                firstLinde = false;
                //Esto Comprueba si estamos en la primera linea, si la primera linea tiene ID en el nombre y realiza el pasaje de string a enum para los identificadores de los idiomas.
                for(int i=0; i<cell.Length; i++)
                {
                    if (!cell[i].Contains("ID"))
                    {
                        try
                        {
                            langColumn[i]=(Language)Enum.Parse(typeof(Language), cell[i]);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Source: {Source}");
                            Debug.LogError($"{e}");
                        }
                        var language = langColumn[i];
                        codex.TryAdd(language, new Dictionary<string, string>());
                    }
                    else
                    {
                        idColum = i;
                    }
                }

                continue;
            }
            for(int i=0;i<cell.Length; i++)
            {
                if (i == idColum) continue;
                if (!langColumn.ContainsKey(i)) continue;
                var language = langColumn[i];
                var id = cell[idColum];
                var textValue = cell[i];
                codex[language][id]= textValue;
            }
        }
        return codex;
    }
}
