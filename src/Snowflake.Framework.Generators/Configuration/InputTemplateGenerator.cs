﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowflake.Configuration.Generators
{
    [Generator]
    public sealed class InputTemplateGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not ConfigurationTemplateInterfaceSyntaxReceiver receiver)
                return;
            bool errorOccured = false;
            var compilation = context.Compilation;
            CSharpParseOptions options = (compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            INamedTypeSymbol configSectionAttr = compilation.GetTypeByMetadataName("Snowflake.Configuration.Attributes.ConfigurationSectionAttribute");
            INamedTypeSymbol configOptionAttr = compilation.GetTypeByMetadataName("Snowflake.Configuration.Attributes.ConfigurationOptionAttribute");
            INamedTypeSymbol inputOptionAttr = compilation.GetTypeByMetadataName("Snowflake.Configuration.Input.InputOptionAttribute");

            INamedTypeSymbol configSectionInterface = compilation.GetTypeByMetadataName("Snowflake.Configuration.IConfigurationSection");
            INamedTypeSymbol configSectionGenericInterface = compilation.GetTypeByMetadataName("Snowflake.Configuration.IConfigurationSection`1");
            INamedTypeSymbol configInstanceAttr = compilation.GetTypeByMetadataName("Snowflake.Configuration.Generators.ConfigurationGenerationInstanceAttribute");
            INamedTypeSymbol guidType = compilation.GetTypeByMetadataName("System.Guid");
            INamedTypeSymbol selectionOptionAttr = compilation.GetTypeByMetadataName("Snowflake.Configuration.Attributes.SelectionOptionAttribute");
            INamedTypeSymbol deviceCapabilityType = compilation.GetTypeByMetadataName("Snowflake.Input.Device.DeviceCapability");

            List<IPropertySymbol> configOptionSymbols = new();
            List<IPropertySymbol> inputOptionSymbols = new();

            foreach (var iface in receiver.CandidateInterfaces)
            {
                var model = compilation.GetSemanticModel(iface.SyntaxTree);
                var ifaceSymbol = model.GetDeclaredSymbol(iface);
                var memberSyntax = iface.Members;

                if (memberSyntax.FirstOrDefault(m => m is not PropertyDeclarationSyntax) is MemberDeclarationSyntax badSyntax)
                {
                    var badSymbol = model.GetDeclaredSymbol(badSyntax);
                    context.ReportError(DiagnosticError.InvalidMembers, "Invalid members in template interface.",
                        $"Template interface '{ifaceSymbol.Name}' must only declare property members. " +
                        $"{badSymbol.Kind} '{ifaceSymbol.Name}.{badSymbol?.Name}' is not a property.",
                        badSyntax.GetLocation(), ref errorOccured);
                    continue;
                }

                if (!iface.Modifiers.Any(p => p.IsKind(SyntaxKind.PartialKeyword)))
                {
                    context.ReportError(DiagnosticError.UnextendibleInterface,
                               "Unextendible template interface",
                               $"Template interface '{ifaceSymbol.Name}' must be marked partial.",
                               iface.GetLocation(), ref errorOccured);
                    continue;
                }

                foreach (var prop in memberSyntax.Cast<PropertyDeclarationSyntax>())
                {
                    var propSymbol = model.GetDeclaredSymbol(prop);

                    if (prop.AccessorList.Accessors.Any(a => a.Body != null || a.ExpressionBody != null))
                    {
                        context.ReportError(DiagnosticError.UnexpectedBody, "Unexpected property body",
                                  $"Property {propSymbol.Name} can not declare a body.",
                              prop.GetLocation(), ref errorOccured);
                        continue;
                    }

                    if (!prop.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)))
                    {
                        context.ReportError(DiagnosticError.MissingSetter, "Missing set accessor",
                                  $"Property {propSymbol.Name} must declare a setter.",
                              prop.GetLocation(), ref errorOccured);
                        continue;
                    }

                    if (!prop.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)))
                    {
                        context.ReportError(DiagnosticError.MissingSetter, "Missing get accessor",
                                $"Property {propSymbol.Name} must declare a getter.",
                            prop.GetLocation(), ref errorOccured);
                        continue;
                    }

                    var configOptionAttrs = propSymbol.GetAttributes()
                            .Where(attr => attr.AttributeClass.Equals(configOptionAttr, SymbolEqualityComparer.Default));
                    var inputTemplateAttrs = propSymbol.GetAttributes()
                            .Where(attr => attr.AttributeClass.Equals(inputOptionAttr, SymbolEqualityComparer.Default));

                    if (!configOptionAttrs.Any() && !inputTemplateAttrs.Any())
                    {
                        context.ReportError(DiagnosticError.UndecoratedProperty, "Undecorated section property member",
                                   $"Property {propSymbol.Name} must be decorated with a ConfigurationOptionAttribute.",
                               prop.GetLocation(), ref errorOccured);
                        continue;
                    }

                    if (configOptionAttrs.Any())
                    {
                        ConfigurationSectionGenerator
                            .VerifyOptionProperty(context, configOptionAttrs.First(), prop, propSymbol, guidType, selectionOptionAttr, ref errorOccured);
                        if (!errorOccured)
                            configOptionSymbols.Add(propSymbol);
                    } 
                    else if (inputTemplateAttrs.Any())
                    {
                        InputTemplateGenerator.VerifyOptionProperty(context, inputTemplateAttrs.First(), prop, propSymbol, deviceCapabilityType, ref errorOccured);
                        if (!errorOccured)
                            inputOptionSymbols.Add(propSymbol);
                    }

              
                }

                if (errorOccured)
                    return;
                string classSource = ProcessClass(ifaceSymbol, configOptionSymbols, inputOptionSymbols, configSectionInterface, configSectionGenericInterface, configInstanceAttr, context);
                context.AddSource($"{ifaceSymbol.Name}_InputTemplateSecion.cs", SourceText.From(classSource, Encoding.UTF8));

            }
        }

        public static void VerifyOptionProperty(
            GeneratorExecutionContext context,
            AttributeData attr, PropertyDeclarationSyntax prop, IPropertySymbol propSymbol,
            INamedTypeSymbol deviceCapabilityType,
            ref bool errorOccured)
        {
            
        }

        public string ProcessClass(INamedTypeSymbol classSymbol, List<IPropertySymbol> configOptionProps,
            List<IPropertySymbol> inputOptionProps,

            INamedTypeSymbol configSectionInterface,
            INamedTypeSymbol configSectionGenericInterface,
            INamedTypeSymbol configInstanceAttr,
            GeneratorExecutionContext context)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                bool errorOccured = false;
                context.ReportError(DiagnosticError.NotTopLevel,
                           "Template interface not top level.",
                           $"Collection template interface {classSymbol.Name} must be defined within an enclosing top-level namespace.",
                           classSymbol.Locations.First(), ref errorOccured);

                return null;
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            string generatedNamespaceName = $"Snowflake.Configuration.GeneratedConfigurationProxies.Section_{namespaceName}";

            string tag = RandomString(6);
            string backingClassName = $"{classSymbol.Name}Proxy_{tag}";
            StringBuilder source = new StringBuilder($@"
namespace {namespaceName}
{{
    [{configInstanceAttr.ToDisplayString()}(typeof({generatedNamespaceName}.{backingClassName}))]
    public partial interface {classSymbol.Name}
    {{
    
    }}

}}

namespace {generatedNamespaceName}
{{
    using System.ComponentModel;
    [EditorBrowsable(EditorBrowsableState.Never)]
    sealed class {backingClassName} : {classSymbol.ToDisplayString()}
    {{
        readonly Snowflake.Configuration.IConfigurationSectionDescriptor __sectionDescriptor;
        readonly Snowflake.Configuration.IConfigurationValueCollection __backingCollection;

        private {backingClassName}(Snowflake.Configuration.IConfigurationSectionDescriptor sectionDescriptor, Snowflake.Configuration.IConfigurationValueCollection collection) 
        {{
            this.__sectionDescriptor = sectionDescriptor;
            this.__backingCollection = collection;
        }}
");

            foreach (var prop in configOptionProps)
            {
                source.Append($@"
{prop.Type.ToDisplayString()} {classSymbol.ToDisplayString()}.{prop.Name}
{{
    get {{ return ({prop.Type.ToDisplayString()})this.__backingCollection[this.__sectionDescriptor, nameof({prop.ToDisplayString()})]?.Value; }}
    set {{ 
            var existingValue = this.__backingCollection[this.__sectionDescriptor, nameof({prop.ToDisplayString()})];
            if (existingValue != null && value != null) {{ existingValue.Value = value; }}
            if (existingValue != null && value == null && this.__sectionDescriptor[nameof({prop.ToDisplayString()})].Type == typeof(string)) 
            {{ existingValue.Value = this.__sectionDescriptor[nameof({prop.ToDisplayString()})].Unset; }}
        }}
}}
");
            }


            source.Append("}}");
            return source.ToString();
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif 
            context.RegisterForSyntaxNotifications(() => new ConfigurationTemplateInterfaceSyntaxReceiver("InputTemplate"));
        }
    }
}
