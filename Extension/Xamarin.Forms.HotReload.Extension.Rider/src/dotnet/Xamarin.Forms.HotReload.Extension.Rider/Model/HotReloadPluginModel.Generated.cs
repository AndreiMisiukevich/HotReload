//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a RdGen v1.07.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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
  
  
  /// <summary>
  /// <p>Generated from: HotReload.Extension.RiderModel.kt:8</p>
  /// </summary>
  public class HotReloadPluginModel : RdExtBase
  {
    //fields
    //public fields
    [NotNull] public ISource<SavedDocument> Reload => _Reload;
    [NotNull] public ISource<bool> Enable => _Enable;
    [NotNull] public IViewableProperty<bool> IsEnabled => _IsEnabled;
    [NotNull] public void ShowMessage(MessageInfo value) => _ShowMessage.Fire(value);
    
    //private fields
    [NotNull] private readonly RdSignal<SavedDocument> _Reload;
    [NotNull] private readonly RdSignal<bool> _Enable;
    [NotNull] private readonly RdProperty<bool> _IsEnabled;
    [NotNull] private readonly RdSignal<MessageInfo> _ShowMessage;
    
    //primary constructor
    private HotReloadPluginModel(
      [NotNull] RdSignal<SavedDocument> reload,
      [NotNull] RdSignal<bool> enable,
      [NotNull] RdProperty<bool> isEnabled,
      [NotNull] RdSignal<MessageInfo> showMessage
    )
    {
      if (reload == null) throw new ArgumentNullException("reload");
      if (enable == null) throw new ArgumentNullException("enable");
      if (isEnabled == null) throw new ArgumentNullException("isEnabled");
      if (showMessage == null) throw new ArgumentNullException("showMessage");
      
      _Reload = reload;
      _Enable = enable;
      _IsEnabled = isEnabled;
      _ShowMessage = showMessage;
      _IsEnabled.OptimizeNested = true;
      BindableChildren.Add(new KeyValuePair<string, object>("reload", _Reload));
      BindableChildren.Add(new KeyValuePair<string, object>("enable", _Enable));
      BindableChildren.Add(new KeyValuePair<string, object>("isEnabled", _IsEnabled));
      BindableChildren.Add(new KeyValuePair<string, object>("showMessage", _ShowMessage));
    }
    //secondary constructor
    internal HotReloadPluginModel (
    ) : this (
      new RdSignal<SavedDocument>(SavedDocument.Read, SavedDocument.Write),
      new RdSignal<bool>(JetBrains.Rd.Impl.Serializers.ReadBool, JetBrains.Rd.Impl.Serializers.WriteBool),
      new RdProperty<bool>(JetBrains.Rd.Impl.Serializers.ReadBool, JetBrains.Rd.Impl.Serializers.WriteBool),
      new RdSignal<MessageInfo>(MessageInfo.Read, MessageInfo.Write)
    ) {}
    //deconstruct trait
    //statics
    
    
    
    protected override long SerializationHash => 8012157446499618967L;
    
    protected override Action<ISerializers> Register => RegisterDeclaredTypesSerializers;
    public static void RegisterDeclaredTypesSerializers(ISerializers serializers)
    {
      
      serializers.RegisterToplevelOnce(typeof(IdeRoot), IdeRoot.RegisterDeclaredTypesSerializers);
    }
    
    
    //constants
    
    //custom body
    //methods
    //equals trait
    //hash code trait
    //pretty print
    public override void Print(PrettyPrinter printer)
    {
      printer.Println("HotReloadPluginModel (");
      using (printer.IndentCookie()) {
        printer.Print("reload = "); _Reload.PrintEx(printer); printer.Println();
        printer.Print("enable = "); _Enable.PrintEx(printer); printer.Println();
        printer.Print("isEnabled = "); _IsEnabled.PrintEx(printer); printer.Println();
        printer.Print("showMessage = "); _ShowMessage.PrintEx(printer); printer.Println();
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
  
  
  /// <summary>
  /// <p>Generated from: HotReload.Extension.RiderModel.kt:15</p>
  /// </summary>
  public sealed class MessageInfo : IPrintable, IEquatable<MessageInfo>
  {
    //fields
    //public fields
    [NotNull] public string Title {get; private set;}
    [NotNull] public string Message {get; private set;}
    
    //private fields
    //primary constructor
    public MessageInfo(
      [NotNull] string title,
      [NotNull] string message
    )
    {
      if (title == null) throw new ArgumentNullException("title");
      if (message == null) throw new ArgumentNullException("message");
      
      Title = title;
      Message = message;
    }
    //secondary constructor
    //deconstruct trait
    public void Deconstruct([NotNull] out string title, [NotNull] out string message)
    {
      title = Title;
      message = Message;
    }
    //statics
    
    public static CtxReadDelegate<MessageInfo> Read = (ctx, reader) => 
    {
      var title = reader.ReadString();
      var message = reader.ReadString();
      var _result = new MessageInfo(title, message);
      return _result;
    };
    
    public static CtxWriteDelegate<MessageInfo> Write = (ctx, writer, value) => 
    {
      writer.Write(value.Title);
      writer.Write(value.Message);
    };
    
    //constants
    
    //custom body
    //methods
    //equals trait
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((MessageInfo) obj);
    }
    public bool Equals(MessageInfo other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Title == other.Title && Message == other.Message;
    }
    //hash code trait
    public override int GetHashCode()
    {
      unchecked {
        var hash = 0;
        hash = hash * 31 + Title.GetHashCode();
        hash = hash * 31 + Message.GetHashCode();
        return hash;
      }
    }
    //pretty print
    public void Print(PrettyPrinter printer)
    {
      printer.Println("MessageInfo (");
      using (printer.IndentCookie()) {
        printer.Print("title = "); Title.PrintEx(printer); printer.Println();
        printer.Print("message = "); Message.PrintEx(printer); printer.Println();
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
  
  
  /// <summary>
  /// <p>Generated from: HotReload.Extension.RiderModel.kt:10</p>
  /// </summary>
  public sealed class SavedDocument : IPrintable, IEquatable<SavedDocument>
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
    //deconstruct trait
    public void Deconstruct([NotNull] out string filePath, [NotNull] out char[] content)
    {
      filePath = FilePath;
      content = Content;
    }
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
    
    //constants
    
    //custom body
    //methods
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
