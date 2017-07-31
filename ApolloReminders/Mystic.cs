using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace bcd
{
    public class Mystic
    {
        #region ***** PROPERTIES *****

        private string _strTemplateBlock;
        private Hashtable _hstValues;
        private Hashtable _ErrorMessage = new Hashtable();
        private string _ParsedBlock;

        private Dictionary<string, Mystic> _Blocks = new Dictionary<string, Mystic>();

        private string VariableTagBegin = "{{";
        private string VariableTagEnd = "}}";

        private string ModifierTag = "|";
        private string ModifierParamSeparator = ",";

        private string ConditionTagIfBegin = "{%If--";
        private string ConditionTagIfEnd = "%}";
        private string ConditionTagElseBegin = "{%Else--";
        private string ConditionTagElseEnd = "%}";
        private string ConditionTagEndIfBegin = "{%EndIf--";
        private string ConditionTagEndIfEnd = "%}";

        private string BlockTagBeginBegin = "{[BlockBegin--";
        private string BlockTagBeginEnd = "]}";
        private string BlockTagEndBegin = "{[BlockEnd--";
        private string BlockTagEndEnd = "]}";

        public string TemplateBlock
        {
            get { return this._strTemplateBlock; }
            set
            {
                this._strTemplateBlock = value;
                ParseBlocks();
            }
        }

        public Hashtable TemplateVars
        {
            get { return this._hstValues; }
            set { this._hstValues = value; }
        }
        public Hashtable ErrorMessage { get; }
        public Dictionary<string, Mystic> Blocks { get; }

        #endregion

        #region ***** CONSTRUCTORS *****

        public Mystic()
        {
            this._strTemplateBlock = String.Empty;
        }

        public Mystic(string filePath)
        {
            ReadTemplateFromFile(filePath);
            ParseBlocks();
        }

        public Mystic(Hashtable templateVars)
        {
            this._hstValues = templateVars;
        }

        public Mystic(string filePath, Hashtable templateVars)
        {
            ReadTemplateFromFile(filePath);
            this._hstValues = templateVars;
        }

        #endregion

        #region ***** PUBLIC METHODS *****

        public void SetTemplateFromFile(string filePath)
        {
            ReadTemplateFromFile(filePath);
        }

        public void SetTemplateFromBlock(string block)
        {
            this.TemplateBlock = block;
        }

        public string Parse()
        {
            ParseConditions();
            ParseVariables();
            return this._ParsedBlock;
        }

        public string ParseBlock(string blockName, Hashtable templateVars)
        {
            if (!this._Blocks.ContainsKey(blockName))
            {
                throw new ArgumentException(String.Format("Could not find Block with Name '{0}'", blockName));
            }

            this._Blocks[blockName].TemplateVars = templateVars;
            return this._Blocks[blockName].Parse();
        }

        public bool ParseToFile(string filePath, bool replaceIfExists)
        {
            if (File.Exists(filePath) && !replaceIfExists)
            {
                return false;
            }
            else
            {
                StreamWriter sr = File.CreateText(filePath);
                sr.Write(Parse());
                sr.Close();
                return true;
            }
        }

        #endregion

        #region ***** PRIVATE METHODS *****

        private void ReadTemplateFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Template file does not exist.");
            }

            StreamReader reader = new StreamReader(filePath);
            this.TemplateBlock = reader.ReadToEnd();
            reader.Close();
        }

        private void ParseBlocks()
        {
            int idxCurrent = 0;
            while ((idxCurrent = this._strTemplateBlock.IndexOf(this.BlockTagBeginBegin, idxCurrent)) != -1)
            {
                string BlockName;
                int idxBlockBeginBegin, idxBlockBeginEnd, idxBlockEndBegin;

                idxBlockBeginBegin = idxCurrent;
                idxCurrent += this.BlockTagBeginBegin.Length;

                // Searching for BlockBeginEnd Index

                idxBlockBeginEnd = this._strTemplateBlock.IndexOf(this.BlockTagBeginEnd, idxCurrent);
                if (idxBlockBeginEnd == -1) throw new Exception("Could not find BlockTagBeginEnd");

                // Getting Block Name

                BlockName = this._strTemplateBlock.Substring(idxCurrent, (idxBlockBeginEnd - idxCurrent));
                idxCurrent = idxBlockBeginEnd + this.BlockTagBeginEnd.Length;

                // Getting End of Block index

                string EndBlockStatment = this.BlockTagEndBegin + BlockName + this.BlockTagEndEnd;
                idxBlockEndBegin = this._strTemplateBlock.IndexOf(EndBlockStatment, idxCurrent);
                if (idxBlockEndBegin == -1) throw new Exception("Could not find End of Block with name '" + BlockName + "'");

                // Add Block to Dictionary

                var block = new Mystic();
                block.TemplateBlock = this._strTemplateBlock.Substring(idxCurrent, (idxBlockEndBegin - idxCurrent));
                this._Blocks.Add(BlockName, block);

                // Remove Block Declaration From Template

                this._strTemplateBlock = this._strTemplateBlock.Remove(idxBlockBeginBegin, (idxBlockEndBegin - idxBlockBeginBegin));

                idxCurrent = idxBlockBeginBegin;
            }
        }

        private void ParseConditions()
        {
            int idxPrevious = 0;
            int idxCurrent = 0;
            this._ParsedBlock = "";
            while ((idxCurrent = this._strTemplateBlock.IndexOf(this.ConditionTagIfBegin, idxCurrent)) != -1)
            {
                string VarName;
                string TrueBlock, FalseBlock;
                string ElseStatment, EndIfStatment;
                int idxIfBegin, idxIfEnd, idxElseBegin, idxEndIfBegin;
                bool boolValue;

                idxIfBegin = idxCurrent;
                idxCurrent += this.ConditionTagIfBegin.Length;

                // Searching for EndIf Index

                idxIfEnd = this._strTemplateBlock.IndexOf(this.ConditionTagIfEnd, idxCurrent);
                if (idxIfEnd == -1) throw new Exception("Could not find ConditionTagIfEnd");

                // Getting Value Name

                VarName = this._strTemplateBlock.Substring(idxCurrent, (idxIfEnd - idxCurrent));

                idxCurrent = idxIfEnd + this.ConditionTagIfEnd.Length;

                // Compare ElseIf and EndIf Indexes

                ElseStatment = this.ConditionTagElseBegin + VarName + this.ConditionTagElseEnd;
                EndIfStatment = this.ConditionTagEndIfBegin + VarName + this.ConditionTagEndIfEnd;
                idxElseBegin = this._strTemplateBlock.IndexOf(ElseStatment, idxCurrent);
                idxEndIfBegin = this._strTemplateBlock.IndexOf(EndIfStatment, idxCurrent);
                if (idxElseBegin > idxEndIfBegin) throw new Exception("Condition Else Tag placed after Condition Tag EndIf for '" + VarName + "'");

                // Getting True and False Condition Blocks

                if (idxElseBegin != -1)
                {
                    TrueBlock = this._strTemplateBlock.Substring(idxCurrent, (idxElseBegin - idxCurrent));
                    FalseBlock = this._strTemplateBlock.Substring((idxElseBegin + ElseStatment.Length), (idxEndIfBegin - idxElseBegin - ElseStatment.Length));
                }
                else
                {
                    TrueBlock = this._strTemplateBlock.Substring(idxCurrent, (idxEndIfBegin - idxCurrent));
                    FalseBlock = "";
                }

                // Parse Condition

                try
                {
                    boolValue = Convert.ToBoolean(this._hstValues[VarName]);
                }
                catch
                {
                    boolValue = false;
                }

                string BeforeBlock = this._strTemplateBlock.Substring(idxPrevious, (idxIfBegin - idxPrevious));

                if (this._hstValues.ContainsKey(VarName) && boolValue)
                {
                    this._ParsedBlock += BeforeBlock + TrueBlock.Trim();
                }
                else
                {
                    this._ParsedBlock += BeforeBlock + FalseBlock.Trim();
                }

                idxCurrent = idxEndIfBegin + EndIfStatment.Length;
                idxPrevious = idxCurrent;
            }
            this._ParsedBlock += this._strTemplateBlock.Substring(idxPrevious);
        }

        private void ParseVariables()
        {
            int idxCurrent = 0;
            while ((idxCurrent = this._ParsedBlock.IndexOf(this.VariableTagBegin, idxCurrent)) != -1)
            {
                string VarName, VarValue;
                int idxVarTagEnd;

                idxVarTagEnd = this._ParsedBlock.IndexOf(this.VariableTagEnd, (idxCurrent + this.VariableTagBegin.Length));
                if (idxVarTagEnd == -1) throw new Exception(String.Format("Index {0}: could not find Variable End Tag", idxCurrent));

                // Getting Variable Name

                VarName = this._ParsedBlock.Substring((idxCurrent + this.VariableTagBegin.Length), (idxVarTagEnd - idxCurrent - this.VariableTagBegin.Length));

                // Checking for Modificators

                string[] VarParts = VarName.Split(this.ModifierTag.ToCharArray());
                VarName = VarParts[0];

                // Getting Variable Value
                // If Variable doesn't exist in _hstValue then
                // Variable Value equal empty string

                // [added 6/6/2006] If variable is null than it will also has empty string

                VarValue = String.Empty;
                if (this._hstValues.ContainsKey(VarName) && this._hstValues[VarName] != null)
                {
                    VarValue = this._hstValues[VarName].ToString();
                }

                // Apply All Modificators to Variable Value

                for (int i = 1; i < VarParts.Length; i++)
                    this.ApplyModifier(ref VarValue, VarParts[i]);

                // Replace Variable in Template

                this._ParsedBlock = this._ParsedBlock.Substring(0, idxCurrent) + VarValue + this._ParsedBlock.Substring(idxVarTagEnd + this.VariableTagEnd.Length);

                // Add Length of added value to Current index 
                // to prevent looking for variables in the added value
                // Fixed Date: April 5, 2006
                idxCurrent += VarValue.Length;
            }
        }

        private void ApplyModifier(ref string value, string modifier)
        {
            // Checking for parameters

            var modifierName = "";
            var parameters = "";
            int idxStartBrackets, idxEndBrackets;

            if ((idxStartBrackets = modifier.IndexOf("(")) != -1)
            {
                idxEndBrackets = modifier.IndexOf(")", idxStartBrackets);
                if (idxEndBrackets == -1)
                {
                    throw new Exception("Incorrect modificator expression");
                }
                else
                {
                    modifierName = modifier.Substring(0, idxStartBrackets).ToUpper();
                    parameters = modifier.Substring(idxStartBrackets + 1, (idxEndBrackets - idxStartBrackets - 1));
                }
            }
            else
            {
                modifierName = modifier.ToUpper();
            }
            string[] arrParameters = parameters.Split(this.ModifierParamSeparator.ToCharArray());
            for (int i = 0; i < arrParameters.Length; i++)
                arrParameters[i] = arrParameters[i].Trim();

            try
            {
                Type typeModifier = Type.GetType("TemplateParser.Modifiers." + modifierName);
                if (typeModifier.IsSubclassOf(Type.GetType("TemplateParser.Modifiers.Modifier")))
                {
                    Modifier objModificator = (Modifier)Activator.CreateInstance(typeModifier);
                    objModificator.Apply(ref value, arrParameters);
                }
            }
            catch
            {
                throw new Exception(String.Format("Could not find modifier '{0}'", modifierName));
            }
        }

        #endregion
    }

    public abstract class Modifier
    {
        protected Hashtable _parameters = new Hashtable();

        public Hashtable Parameters
        {
            get { return _parameters; }
        }

        public abstract void Apply(ref string value, params string[] Parameters);
    }

    internal class NL2BR : Modifier
    {
        public override void Apply(ref string value, params string[] Parameters)
        {
            value = value.Replace("\n", "<br>");
        }
    }

    internal class UPPER : Modifier
    {
        public override void Apply(ref string value, params string[] Parameters)
        {
            value = value.ToUpper();
        }
    }

    internal class LOWER : Modifier
    {
        public override void Apply(ref string value, params string[] Parameters)
        {
            value = value.ToLower();
        }
    }

    internal class TRIM : Modifier
    {
        public override void Apply(ref string value, params string[] Parameters)
        {
            value = value.Trim();
        }
    }

    internal class TRIMSTART : Modifier
    {
        public override void Apply(ref string value, params string[] Parameters)
        {
            value = value.TrimStart();
        }
    }

    internal class TRIMEND : Modifier
    {
        public override void Apply(ref string value, params string[] Parameters)
        {
            value = value.TrimEnd();
        }
    }
}
