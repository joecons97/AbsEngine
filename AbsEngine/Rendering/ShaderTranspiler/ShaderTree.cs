namespace AbsEngine.Rendering.ShaderTranspiler
{
    [Serializable]
    public class ShaderTree
    {
        [Serializable]
        public class Directive
        {
            public string Type { get; set; } = "";
            public string Name { get; set; } = "";
            public string[] Params { get; set; } = Array.Empty<string>();

            public override string ToString()
            {
                return $"#{Type} {Name} {string.Join(' ', Params)};";
            }
        }

        [Serializable]
        public class Type
        {
            public string Name { get; set; } = "";
            public List<Member> Members { get; set; } = new List<Member>();

            public override string ToString()
            {
                return $"struct {Name}\n{{\n{string.Join('\n', Members.Select(x => $"{x.Type} {x.Name};"))}\n}};";
            }
        }

        [Serializable]
        public class Member
        {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public string DefaultValue { get; set; } = "";

            public override string ToString()
            {
                if (string.IsNullOrEmpty(DefaultValue))
                    return $"{Type} {Name};";

                return $"{Type} {Name} = {DefaultValue};";
            }
        }

        [Serializable]
        public class Function
        {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public List<Member> Params { get; set; } = new List<Member>();
            public List<string> CodeLines { get; set; } = new List<string>();

            public override string ToString()
            {
                return $"{Type} {Name}" +
                    $"({string.Join(',', Params.Select(x => x.ToString().TrimEnd(';')))}) " +
                    $"{{\n" +
                    $"{string.Join('\n', CodeLines)}" +
                    $"}}";
            }
        }

        public List<Directive> Directives { get; set; } = new List<Directive>();
        public List<Type> Types { get; set; } = new List<Type>();
        public List<Member> Members { get; set; } = new List<Member>();
        public List<Function> Functions { get; set; } = new List<Function>();

        static Queue<T> ToQueue<T>(IEnumerable<T> list)
        {
            var queue = new Queue<T>();
            foreach (var item in list)
            {
                queue.Enqueue(item);
            }
            return queue;
        }

        public static ShaderTree? GetParseFile(string path)
        {
            var text = ToQueue(File.ReadAllText(path));
            var shaderObject = new ShaderTree();

            string defType = "";
            string defName = "";
            string defLines = "";
            object? activeDef = null;

            while (text.Count > 0)
            {
                var c = text.Dequeue();
                if (c == '\n' || c == '\r')
                    continue;

                if (activeDef == null)
                {
                    if (c == ' ')
                    {
                        if (defType == "struct")
                        {
                            activeDef = new Type();
                            shaderObject.Types.Add((Type)activeDef);
                        }
                        else if (defType.StartsWith("#"))
                        {
                            var dir = new Directive();
                            shaderObject.Directives.Add(dir);
                            dir.Type = defType.TrimStart('#');
                            activeDef = dir;
                        }
                        else if (!string.IsNullOrEmpty(defType))
                        {
                            //Could be either a member or a function
                            //We'll assume it's a member for now but we can change it later is need be
                            var mem = new Member();
                            mem.Type = defType;
                            activeDef = mem;
                        }
                    }

                    defType += c;
                }
                else
                {
                    if (activeDef is Directive dir)
                    {
                        if (c == ' ')
                        {
                            dir.Name = defName;
                        }

                        if (string.IsNullOrEmpty(dir.Name))
                            defName += c;
                        else
                        {
                            if (c == ';')
                            {
                                defLines = defLines.Trim();
                                dir.Params = defLines.Split(' ');
                                activeDef = null;
                                defLines = "";
                                defType = "";
                                defName = "";
                                continue;
                            }

                            defLines += c;
                        }
                    }
                    else if (activeDef is Type type)
                    {
                        if (c == '{')
                        {
                            if (string.IsNullOrEmpty(type.Name) == false)
                                throw new Exception($"Unexpected char \'{c}\'");

                            type.Name = defName.Trim();
                            continue;
                        }

                        if (string.IsNullOrEmpty(type.Name))
                            defName += c;
                        else
                        {
                            if (c == '}' && text.Dequeue() == ';')
                            {
                                activeDef = null;
                                defLines = "";
                                defType = "";
                                defName = "";
                                continue;
                            }
                            else if (c == ';')
                            {
                                var split = defLines.Replace(";", "").Trim().Split(' ');
                                type.Members.Add(new Member()
                                {
                                    Type = split[0].Trim().TrimEnd(';'),
                                    Name = split[1].Trim().TrimEnd(';'),
                                });
                                defLines = "";
                            }

                            defLines += c;
                        }
                    }
                    else if (activeDef is Member mem)
                    {
                        if (c == ';')
                        {
                            if (string.IsNullOrEmpty(mem.Name) == false)
                                throw new Exception($"Unexpected char \'{c}\'");

                            mem.Name = defName;
                            shaderObject.Members.Add(mem);
                            activeDef = null;
                            defLines = "";
                            defType = "";
                            defName = "";
                            continue;
                        }
                        else if (c == '(')
                        {
                            var f = new Function()
                            {
                                Name = defName,
                                Type = mem.Type,
                            };
                            activeDef = f;
                            shaderObject.Functions.Add(f);

                            defLines = "";
                            defType = "";
                            defName = "";
                            continue;
                            //Actually a function...
                        }

                        if (string.IsNullOrEmpty(mem.Type))
                            defType += c;
                        else if (string.IsNullOrEmpty(mem.Name))
                            defName += c;
                    }
                    else if (activeDef is Function func)
                    {
                        if (c == ')' && func.Params.Count == 0)
                        {
                            var paramsString = defLines.Trim().TrimEnd(')').Split(',').Select(x => x.Trim());
                            var param = paramsString.Select(x => new Member() { Type = x.Split(' ')[0].Trim(), Name = x.Split(' ')[1].Trim() });
                            func.Params = param.ToList();

                            defLines = "";
                            defType = "";
                            defName = "";

                            continue;
                        }
                        else if (c == '}')
                        {
                            var lines = defLines
                                .TrimEnd(')')
                                .Trim()
                                .Split(';')
                                .Select(x => string.IsNullOrEmpty(x) ? "" : x.Trim() + ";")
                                .Where(x => !x.StartsWith("//"));

                            func.CodeLines = lines.ToList();

                            activeDef = null;
                            defLines = "";
                            defType = "";
                            defName = "";
                        }
                        else if (c == '{')
                            continue;

                        if (func.Params.Count == 0 || func.CodeLines.Count == 0)
                        {
                            defLines += c;
                        }
                    }
                }
            }

            return shaderObject;
        }
    }
}
