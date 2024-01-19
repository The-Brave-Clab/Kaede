using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Y3ADV
{
    public class Y3ScriptFunction
    {
        private Y3ScriptModule scriptModule = null;
        public string functionName;
        private List<string> statements = null;
        private List<Parameter> parameters = null;

        public List<string> Statements => statements;

        private class Parameter
        {
            public int index;
            public string name;

            public Parameter(int index, string name)
            {
                this.index = index;
                this.name = name;
            }
        }

        public Y3ScriptFunction(Y3ScriptModule module, string definitionStatement)
        {
            scriptModule = module;
            
            var trimmed = definitionStatement.Trim();
            var split = trimmed.Split('\t');

            functionName = split[1];

            parameters = new List<Parameter>(split.Length - 2);
            statements = new List<string>();
            
            for (int i = 2; i < split.Length; ++i)
            {
                parameters.Add(new Parameter(i - 2, split[i]));
            }
        }

        public void FinishDefinition()
        {
            parameters.Sort((p2, p1) => p1.name.Length.CompareTo(p2.name.Length));
        }

        public void AddStatement(string statement)
        {
            statements.Add(statement);
        }

        public List<string> GetStatements(List<string> parameterValues)
        {
            if (parameterValues.Count != parameters.Count)
            {
                Debug.LogError($"Error calling function {functionName}! Number of desired parameters ({parameterValues.Count}) doesn't fit with defined ({parameters.Count}).");
                return null;
            }

            List<string> realStatements = new List<string>(statements.Count);
            
            foreach (var statement in statements)
            {
                string realStatement = statement;

                for (int i = 0; i < parameters.Count; ++i)
                {
                    foreach (var parameter in parameters.Where(parameterPair => parameterPair.index == i))
                    {
                        realStatement = realStatement.Replace(parameter.name, parameterValues[i]);
                        break;
                    }
                }

                realStatements.Add(realStatement);
            }

            return realStatements;
        }
    }
}