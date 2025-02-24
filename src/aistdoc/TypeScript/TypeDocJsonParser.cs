﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace aistdoc
{

    /// <summary>
    ///  
    /// </summary>
    internal class TypeDocPatserException : Exception {
        public TypeDocPatserException(string message) : base(message) {

        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class TypeDocJsonParser {

        private readonly List<string> _files = new List<string>();

        private TypeScriptLibrary _lib;

        public TypeDocJsonParser(IEnumerable<string> files)
        {
            _files.AddRange(files);
        }

        public TypeScriptLibrary Parse()
        {
            if (_lib == null) {
                _lib = new TypeScriptLibrary();

                JObject jobject;
                foreach (var file in _files) {
                    if (File.Exists(file)) {
                        jobject = JObject.Parse(File.ReadAllText(file));

                        var package = new TypeScriptPackage();
                        LoadFromJObject(package, jobject);

                        _lib.Packages.Add(package);
                    }
                }
            }

            return _lib;

        }

        private void LoadFromJObject(TypeScriptPackage package, JObject jobject)
        {

            if (jobject.TryGetValue("name", out var nameToken))
            {
                package.Name = nameToken.ToString();
            }


            if (jobject.TryGetValue("children", out var childrenToken))
            {

                //expects here extenral modules
                var children = childrenToken.ToObject<List<JObject>>();


                foreach (var child in children)
                {
                    var childKind = child["kind"].ToObject<TypeScriptTokenKind>();
                    if (childKind == TypeScriptTokenKind.Class)
                    {
                        var @class = new TypeScriptClass(package);
                        LoadFromJObject(@class, child);
                        package.Classes.Add(@class);
                    }
                    else if (childKind == TypeScriptTokenKind.Interface)
                    {
                        var @interface = new TypeScriptInterface(package);
                        LoadFromJObject(@interface, child);
                        package.Interfaces.Add(@interface);
                    }
                    else if (childKind == TypeScriptTokenKind.Function)
                    {
                        var function = new TypeScriptFunction(package);
                        LoadFromJObject(function, child);
                        package.Functions.Add(function);
                    }
                    else if (childKind == TypeScriptTokenKind.Namespace)
                    {
                        var @namespace = new TypeScriptNamespace(package);
                        LoadFromJObject(@namespace, child);
                        package.Namespaces.Add(@namespace);
                    }
                    else if (childKind == TypeScriptTokenKind.Enumeration)
                    {
                        var @enum = new TypeScriptEnumeration(package);
                        LoadFromJObject(@enum, child);
                        package.Enumerations.Add(@enum);
                    }
                    else if (childKind == TypeScriptTokenKind.Varialbe)
                    {
                        var @var = new TypeScriptVariable(package);
                        LoadFromJObject(var, child);
                        package.Variables.Add(@var);
                    }
                }


                if (jobject.TryGetValue("comment", out var commentToken))
                {
                    package.Comment = new TypeScriptComment();
                    LoadFromJObject(package.Comment, commentToken.ToObject<JObject>());
                }
            }
        }

        private void LoadFromJObject(TypeScriptNamespace @namespace, JObject jobject) {
            if (jobject.TryGetValue("name", out var nameToken)) {
                @namespace.Name = nameToken.ToString(); ;
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isExported", out var isExportedToken)) {
                    @namespace.IsExported = isExportedToken.ToObject<bool>();
                }
            }

            if (jobject.TryGetValue("children", out var childrenToken)) {
                var children = childrenToken.ToObject<List<JObject>>();

                foreach (var child in children) {
                    var childKind = child["kind"].ToObject<TypeScriptTokenKind>();
                    if (childKind == TypeScriptTokenKind.Class) {
                        var @class = new TypeScriptClass(@namespace);
                        LoadFromJObject(@class, child);
                        @namespace.Classes.Add(@class);
                    }
                    else if (childKind == TypeScriptTokenKind.Interface) {
                        var @interface = new TypeScriptInterface(@namespace);
                        LoadFromJObject(@interface, child);
                        @namespace.Interfaces.Add(@interface);
                    }
                    else if (childKind == TypeScriptTokenKind.Function) {
                        var function = new TypeScriptFunction(@namespace);
                        LoadFromJObject(function, child);
                        @namespace.Functions.Add(function);
                    }
                    else if (childKind == TypeScriptTokenKind.Namespace) {
                        var nspace= new TypeScriptNamespace(@namespace);
                        LoadFromJObject(nspace, child);
                        @namespace.Namespaces.Add(nspace);
                    }
                    else if (childKind == TypeScriptTokenKind.Enumeration) {
                        var @enum = new TypeScriptEnumeration(@namespace);
                        LoadFromJObject(@enum, child);
                        @namespace.Enumerations.Add(@enum);
                    }
                    else if (childKind == TypeScriptTokenKind.Varialbe) {
                        var @var = new TypeScriptVariable(@namespace);
                        LoadFromJObject(var, child);
                        @namespace.Variables.Add(@var);
                    }
                }
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                @namespace.Comment = new TypeScriptComment();
                LoadFromJObject(@namespace.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptVariable variable, JObject jobject)
        {
            if (jobject.TryGetValue("name", out var nameTokent)) {
                variable.Name = nameTokent.ToString();
            }

            if (jobject.TryGetValue("flags", out var flagsToken))
            {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isExported", out var isExportedToken)) {
                    variable.IsExported = isExportedToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isLet", out var isLetToken)) {
                    variable.IsLet = isLetToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isConst", out var isConstToken)) {
                    variable.IsConst = isConstToken.ToObject<bool>();
                }
            }

            if (jobject.TryGetValue("type", out var typeToken)) {
                variable.Type = LoadTypeFromJObject(typeToken.ToObject<JObject>());
            }

            if (jobject.TryGetValue("defaultValue", out var defValToken)) {
                variable.DefaultValue = defValToken.ToString();
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                variable.Comment = new TypeScriptComment();
                LoadFromJObject(variable.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptFunction function, JObject jobject)
        {
            if (jobject.TryGetValue("name", out var nameToken)) {
                function.Name = nameToken.ToString(); ;
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isExported", out var isExportedToken)){
                    function.IsExported = isExportedToken.ToObject<bool>();
                }
            }

            if (jobject.TryGetValue("signatures", out var signatureToken)) {
                LoadFromJObject(function.Signature, signatureToken.ToObject<List<JObject>>().First());
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                function.Comment = new TypeScriptComment();
                LoadFromJObject(function.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptEnumeration @enum, JObject jobject)
        {

            if (jobject.TryGetValue("id", out var idToken)) {
                @enum.Id = idToken.ToObject<int>();
            }

            if (jobject.TryGetValue("name", out var nameToken)) {
                @enum.Name = nameToken.ToObject<string>();
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isExported", out var isExportedToken)) {
                    @enum.IsExported = isExportedToken.ToObject<bool>();
                }

            }

            if (jobject.TryGetValue("children", out var childrenToken)) {
                var children = childrenToken.ToObject<List<JObject>>();

                foreach (var child in children) {
                    var member = new TypeScriptEnumerationMember();
                    LoadFromJObject(member, child);
                    @enum.Members.Add(member);
                }
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                @enum.Comment = new TypeScriptComment();
                LoadFromJObject(@enum.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptEnumerationMember member, JObject jobject)
        {
            if (jobject.TryGetValue("name", out var nameToken)) {
                member.Name = nameToken.ToObject<string>();
            }

            if (jobject.TryGetValue("defaultValue", out var defValToken)) {
                member.DefaultValue = defValToken.ToObject<string>();
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                member.Comment = new TypeScriptComment();
                LoadFromJObject(member.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptInterface @interface, JObject jobject)
        {

            if (jobject.TryGetValue("id", out var idTokent)) {
                @interface.Id = idTokent.ToObject<int>();
            }

            if (jobject.TryGetValue("name", out var nameTokent)) {
                @interface.Name = nameTokent.ToString();
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isExported", out var isExportedToken)) {
                    @interface.IsExported = isExportedToken.ToObject<bool>();
                }

            }

            if (jobject.TryGetValue("children", out var childrenToken)) {
                var children = childrenToken.ToObject<List<JObject>>();

                foreach (var child in children) {
                    var childKind = child["kind"].ToObject<TypeScriptTokenKind>();
                    if (childKind == TypeScriptTokenKind.Property) {
                        var property = new TypeScriptProperty();
                        LoadFromJObject(property, child);
                        @interface.Properties.Add(property);
                    }
                    else if (childKind == TypeScriptTokenKind.Method) {
                        var method = new TypeScriptMethod();
                        LoadFromJObject(method, child);
                        @interface.Methods.Add(method);
                    }
                }
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                @interface.Comment = new TypeScriptComment();
                LoadFromJObject(@interface.Comment, commentToken.ToObject<JObject>());
            }

            if (jobject.TryGetValue("extendedTypes", out var extendedTypesToken)) {
                var typeObjs = extendedTypesToken.ToObject<List<JObject>>();
                foreach (var typeObj in typeObjs) {
                    @interface.ExtendedTypes.Add(LoadTypeFromJObject(typeObj));
                }

            }
        }

        private void LoadFromJObject(TypeScriptClass @class, JObject jobject)
        {
            if (jobject.TryGetValue("id", out var idToken)){
                @class.Id = idToken.ToObject<int>();
            }

            if (jobject.TryGetValue("name", out var nameTokent)) {
                @class.Name = nameTokent.ToString();
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isExported", out var isExportedToken)) {
                    @class.IsExported = isExportedToken.ToObject<bool>();
                }

            }

            if (jobject.TryGetValue("children", out var childrenToken)) {
                var children = childrenToken.ToObject<List<JObject>>();

                foreach (var child in children) {
                    var childKind = child["kind"].ToObject<TypeScriptTokenKind>();
                    if (childKind == TypeScriptTokenKind.Property) {
                        var property = new TypeScriptProperty();
                        LoadFromJObject(property, child);
                        @class.Properties.Add(property);
                    }
                    else if (childKind == TypeScriptTokenKind.Method) {
                        var method = new TypeScriptMethod();
                        LoadFromJObject(method, child);
                        @class.Methods.Add(method);
                    }
                    else if (childKind == TypeScriptTokenKind.Constructor) {
                        @class.Constructor = new TypeScriptMethod();
                        LoadFromJObject(@class.Constructor,child);
                    }
                }
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                @class.Comment = new TypeScriptComment();
                LoadFromJObject(@class.Comment, commentToken.ToObject<JObject>());
            }

            if (jobject.TryGetValue("implementedTypes", out var impTypesToken)) {
                var typeObjs = impTypesToken.ToObject<List<JObject>>();
                foreach (var typeObj in typeObjs) {
                    @class.ImplementedTypes.Add(LoadTypeFromJObject(typeObj));
                }

            }

            if (jobject.TryGetValue("extendedTypes", out var exToken)) {
                var typeObjs = exToken.ToObject<List<JObject>>();
                foreach (var typeObj in typeObjs) {
                    @class.ExtendedTypes.Add(LoadTypeFromJObject(typeObj));
                }

            }
        }

        private void LoadFromJObject(TypeScriptProperty property, JObject jobject)
        {
            if (jobject.TryGetValue("name", out var nameTokent)) {
                property.Name = nameTokent.ToString();
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isPublic", out var isPublicToken)) {
                    property.IsPublic = isPublicToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isProtected", out var isProtectedToken)) {
                    property.IsProtected = isProtectedToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isOptional", out var isOptionalToken)) {
                    property.IsOptional = isOptionalToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isStatic", out var isStaticToken)) {
                    property.IsStatic = isStaticToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isPrivate", out var isPrivateToken)) {
                    property.IsPrivate = isPrivateToken.ToObject<bool>();
                }

                if (jobject.TryGetValue("type", out var typeToken)) {
                    property.Type = LoadTypeFromJObject(typeToken.ToObject<JObject>());
                }
            }

            if (jobject.TryGetValue("defaultValue", out var defValToken)) {
                property.DefaultValue = defValToken.ToString();
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                property.Comment = new TypeScriptComment();
                LoadFromJObject(property.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptMethod method, JObject jobject)
        {
            if (jobject.TryGetValue("name", out var nameTokent)) {
                method.Name = nameTokent.ToString();
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isPublic", out var isPublicToken)) {
                    method.IsPublic = isPublicToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isProtected", out var isProtectedToken)) {
                    method.IsProtected = isProtectedToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isPrivate", out var isPrivateToken)) {
                    method.IsPrivate = isPrivateToken.ToObject<bool>();
                }

                if (flagsObj.TryGetValue("isOptional", out var isOptionalToken)) {
                    method.IsOptional = isOptionalToken.ToObject<bool>();

                }

                if (flagsObj.TryGetValue("isStatic", out var isStaticToken)) {
                    method.IsStatic = isStaticToken.ToObject<bool>();
                }

            }
            if (jobject.TryGetValue("signatures", out var signatureToken)) {
                LoadFromJObject(method.Signature, signatureToken.ToObject<List<JObject>>().First());
            }
        }

        private void LoadFromJObject(TypeScriptSignature signature, JObject jobject)
        {
            if (jobject.TryGetValue("name", out var nameToken)) {
                signature.Name = nameToken.ToString();
            }

            if (jobject.TryGetValue("parameters", out var parametersToken)) {
                var parameterObjs = parametersToken.ToObject<List<JObject>>();

                foreach (var paramObj in parameterObjs) {
                    var parameter = new TypeScriptParameter();
                    LoadFromJObject(parameter, paramObj);
                    signature.Parameters.Add(parameter);
                }
            }

            if (jobject.TryGetValue("type", out var typeToken)) {
                signature.Type = LoadTypeFromJObject(typeToken.ToObject<JObject>());
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                signature.Comment = new TypeScriptComment();
                LoadFromJObject(signature.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptParameter parameter, JObject jobject)
        {
            if (jobject.TryGetValue("name", out var nameTokent)) {
                parameter.Name = nameTokent.ToString();
            }

            if (jobject.TryGetValue("flags", out var flagsToken)) {
                var flagsObj = flagsToken.ToObject<JObject>();

                if (flagsObj.TryGetValue("isOptional", out var isOptionalToken)) {
                    parameter.IsOptional = isOptionalToken.ToObject<bool>();
                }
            }

            if (jobject.TryGetValue("type", out var typeToken)) {
                parameter.Type = LoadTypeFromJObject(typeToken.ToObject<JObject>());
            }

            if (jobject.TryGetValue("defaultValue", out var defValToken)) {
                parameter.DefaultValue = defValToken.ToString();
            }

            if (jobject.TryGetValue("comment", out var commentToken)) {
                parameter.Comment = new TypeScriptComment();
                LoadFromJObject(parameter.Comment, commentToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptReflectionType type, JObject jobject)
        {
            try {
                var signatureObj = jobject["declaration"]["signatures"].ToObject<List<JObject>>().First();
                type.Signature = new TypeScriptSignature();
                LoadFromJObject(type.Signature, signatureObj);
            }
            catch {
              
            }
        }

        private void LoadFromJObject(TypeScriptArrayType type, JObject jobject)
        {
            if (jobject.TryGetValue("elementType", out var elementTypeToken)) {
                type.ElementType = LoadTypeFromJObject(elementTypeToken.ToObject<JObject>());
            }
        }

        private void LoadFromJObject(TypeScriptReferenceType type, JObject jobject)
        {

            if (jobject.TryGetValue("id", out var idToken)) {
                type.Id = idToken.ToObject<int>();
            }

            if (jobject.TryGetValue("typeArguments", out var typeArgumentsToken)) {
                var typeArgumentObjs = typeArgumentsToken.ToObject<List<JObject>>();
                foreach (var typeArgObj in typeArgumentObjs) {
                    type.TypeArguments.Add(LoadTypeFromJObject(typeArgObj));
                }
            }
        }

        private void LoadFromJObject(TypeScriptUnionType type, JObject jobject)
        {

            if (jobject.TryGetValue("types", out var typesToken)) {

                var typeObjs = typesToken.ToObject<List<JObject>>();

                foreach (var typeObj in typeObjs) {
                    type.Types.Add(LoadTypeFromJObject(typeObj));
                }
            }
        }

        private TypeScriptType LoadTypeFromJObject(JObject jobject)
        {

            var typeStr = "";
            if (jobject.TryGetValue("type", out var typeToken)) {
                typeStr = typeToken.ToObject<string>();
            }

            var type = TypeScriptType.CreateTypeSctiptType(typeStr);

            if (jobject.TryGetValue("name", out var nameToken)) {
                type.Name = nameToken.ToObject<string>();
            }

            if (type is TypeScriptReflectionType) {
                LoadFromJObject((TypeScriptReflectionType)type, jobject);
            }
            else if (type is TypeScriptArrayType) {
                LoadFromJObject((TypeScriptArrayType)type, jobject);
            }
            else if (type is TypeScriptUnionType) {
                LoadFromJObject((TypeScriptUnionType)type, jobject);
            }
            else if (type is TypeScriptReferenceType) {
                LoadFromJObject((TypeScriptReferenceType)type, jobject);
            }
            else if (type is TypesScriptStringLiteralType) {
                LoadFromJObject((TypesScriptStringLiteralType)type, jobject);
            }

            return type;
           
        }

        private void LoadFromJObject(TypesScriptStringLiteralType type, JObject jobject)
        {

            if (jobject.TryGetValue("value", out var valueToken)) {
                type.Value = valueToken.ToObject<string>();
            }

        }

        private void LoadFromJObject(TypeScriptComment comment, JObject jobject)
        {
            if (jobject.TryGetValue("shortText", out var shortTextToken)) {
                comment.ShortText = shortTextToken.ToString();
            }

            if (jobject.TryGetValue("text", out var textToken)) {
                comment.Text = textToken.ToString();
            }

            if (jobject.TryGetValue("returns", out var returnsToken)) {
                comment.Returns = returnsToken.ToString();
            }

            if (jobject.TryGetValue("tags", out var tagsToken)) {
                var tagsObj = tagsToken.ToObject<List<JObject>>();
                foreach (var tag in tagsObj) {
                    comment.Tags.Add(tag["tag"].ToString(), tag["text"].ToString());
                }
            }
        }
    }
}
