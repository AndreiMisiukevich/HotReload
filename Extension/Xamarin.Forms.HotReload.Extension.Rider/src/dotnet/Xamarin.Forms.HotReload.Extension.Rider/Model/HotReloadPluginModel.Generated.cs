using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

using JetBrains.Core;
using JetBrains.Diagnostics;
using JetBrains.Collections;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using JetBrains.Serialization;
using JetBrains.Rd;
using JetBrains.Rd.Base;
using JetBrains.Rd.Impl;
using JetBrains.Rd.Tasks;
using JetBrains.Rd.Util;
using JetBrains.Rd.Text;


// ReSharper disable RedundantEmptyObjectCreationArgumentList
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantOverflowCheckingContext


namespace JetBrains.Rider.Model
{
  
  
  public class HotReloadPluginModel : RdExtBase
  {
    //fields
    //public fields
    [NotNull] public ISource<SavedDocument> Reload => _Reload;
    [NotNull] public IViewableProperty<bool> IsEnabled => _IsEnabled;
    
    //private fields
    [NotNull] private readonly RdSignal<SavedDocument> _Reload;
    [NotNull] private readonly RdProperty<bool> _IsEnabled;
    
    //primary constructor
    private HotReloadPluginModel(
      [NotNull] RdSignal<SavedDocument> reload,
      [NotNull] RdProperty<bool> isEnabled
    )
    {
      if (reload == null) throw new ArgumentNullException("reload");
      if (isEnabled == null) throw new ArgumentNullException("isEnabled");
      
      _Reload = reload;
      _IsEnabled = isEnabled;
      _IsEnabled.OptimizeNested = true;
      BindableChildren.Add(new KeyValuePair<string, object>("reload", _Reload));
      BindableChildren.Add(new KeyValuePair<string, object>("isEnabled", _IsEnabled));
    }
    //secondary constructor
    internal HotReloadPluginModel (
    ) : this (
      new RdSignal<SavedDocument>(SavedDocument.Read, SavedDocument.Write),
      new RdProperty<bool>(JetBrains.Rd.Impl.Serializers.ReadBool, JetBrains.Rd.Impl.Serializers.WriteBool)
    ) {}
    //statics
    
    
    
    protected override long SerializationHash => 4156314565325463560L;
    
    protected override Action<ISerializers> Register => RegisterDeclaredTypesSerializers;
    public static void RegisterDeclaredTypesSerializers(ISerializers serializers)
    {
      
      serializers.RegisterToplevelOnce(typeof(IdeRoot), IdeRoot.RegisterDeclaredTypesSerializers);
    }
    
    //custom body
    //equals trait
    //hash code trait
    //pretty print
    public override void Print(PrettyPrinter printer)
    {
      printer.Println("HotReloadPluginModel (");
      using (printer.IndentCookie()) {
        printer.Print("reload = "); _Reload.PrintEx(printer); printer.Println();
        printer.Print("isEnabled = "); _IsEnabled.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
  public static class SolutionHotReloadPluginModelEx
   {
    public static HotReloadPluginModel GetHotReloadPluginModel(this Solution solution)
    {
      return solution.GetOrCreateExtension("hotReloadPluginModel", () => new HotReloadPluginModel());
    }
  }
  
  
  public class SavedDocument : IPrintable, IEquatable<SavedDocument>
  {
    //fields
    //public fields
    [NotNull] public string FilePath {get; private set;}
    [NotNull] public char[] Content {get; private set;}
    
    //private fields
    //primary constructor
    public SavedDocument(
      [NotNull] string filePath,
      [NotNull] char[] content
    )
    {
      if (filePath == null) throw new ArgumentNullException("filePath");
      if (content == null) throw new ArgumentNullException("content");
      
      FilePath = filePath;
      Content = content;
    }
    //secondary constructor
    //statics
    
    public static CtxReadDelegate<SavedDocument> Read = (ctx, reader) => 
    {
      var filePath = reader.ReadString();
      var content = JetBrains.Rd.Impl.Serializers.ReadCharArray(ctx, reader);
      var _result = new SavedDocument(filePath, content);
      return _result;
    };
    
    public static CtxWriteDelegate<SavedDocument> Write = (ctx, writer, value) => 
    {
      writer.Write(value.FilePath);
      JetBrains.Rd.Impl.Serializers.WriteCharArray(ctx, writer, value.Content);
    };
    //custom body
    //equals trait
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((SavedDocument) obj);
    }
    public bool Equals(SavedDocument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return FilePath == other.FilePath && Content.SequenceEqual(other.Content);
    }
    //hash code trait
    public override int GetHashCode()
    {
      unchecked {
        var hash = 0;
        hash = hash * 31 + FilePath.GetHashCode();
        hash = hash * 31 + Content.ContentHashCode();
        return hash;
      }
    }
    //pretty print
    public void Print(PrettyPrinter printer)
    {
      printer.Println("SavedDocument (");
      using (printer.IndentCookie()) {
        printer.Print("filePath = "); FilePath.PrintEx(printer); printer.Println();
        printer.Print("content = "); Content.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
}
