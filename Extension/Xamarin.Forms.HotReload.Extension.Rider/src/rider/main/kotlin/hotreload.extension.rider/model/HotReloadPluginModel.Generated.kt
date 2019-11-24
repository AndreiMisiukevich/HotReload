@file:Suppress("EXPERIMENTAL_API_USAGE","EXPERIMENTAL_UNSIGNED_LITERALS","PackageDirectoryMismatch","UnusedImport","unused","LocalVariableName","CanBeVal","PropertyName","EnumEntryName","ClassName","ObjectPropertyName","UnnecessaryVariable")
package com.jetbrains.rider.model

import com.jetbrains.rd.framework.*
import com.jetbrains.rd.framework.base.*
import com.jetbrains.rd.framework.impl.*

import com.jetbrains.rd.util.lifetime.*
import com.jetbrains.rd.util.reactive.*
import com.jetbrains.rd.util.string.*
import com.jetbrains.rd.util.*
import kotlin.reflect.KClass



/**
 * #### Generated from [HotReload.Extension.RiderModel.kt:8]
 */
class HotReloadPluginModel private constructor(
    private val _reload: RdSignal<SavedDocument>,
    private val _enable: RdSignal<Boolean>,
    private val _isEnabled: RdOptionalProperty<Boolean>,
    private val _showMessage: RdSignal<MessageInfo>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers)  {
            serializers.register(SavedDocument)
            serializers.register(MessageInfo)
        }
        
        
        
        
        const val serializationHash = 8012157446499618967L
        
    }
    override val serializersOwner: ISerializersOwner get() = HotReloadPluginModel
    override val serializationHash: Long get() = HotReloadPluginModel.serializationHash
    
    //fields
    val reload: ISignal<SavedDocument> get() = _reload
    val enable: ISignal<Boolean> get() = _enable
    val isEnabled: IOptProperty<Boolean> get() = _isEnabled
    val showMessage: ISource<MessageInfo> get() = _showMessage
    //initializer
    init {
        _isEnabled.optimizeNested = true
    }
    
    init {
        bindableChildren.add("reload" to _reload)
        bindableChildren.add("enable" to _enable)
        bindableChildren.add("isEnabled" to _isEnabled)
        bindableChildren.add("showMessage" to _showMessage)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdSignal<SavedDocument>(SavedDocument),
        RdSignal<Boolean>(FrameworkMarshallers.Bool),
        RdOptionalProperty<Boolean>(FrameworkMarshallers.Bool),
        RdSignal<MessageInfo>(MessageInfo)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("HotReloadPluginModel (")
        printer.indent {
            print("reload = "); _reload.print(printer); println()
            print("enable = "); _enable.print(printer); println()
            print("isEnabled = "); _isEnabled.print(printer); println()
            print("showMessage = "); _showMessage.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): HotReloadPluginModel   {
        return HotReloadPluginModel(
            _reload.deepClonePolymorphic(),
            _enable.deepClonePolymorphic(),
            _isEnabled.deepClonePolymorphic(),
            _showMessage.deepClonePolymorphic()
        )
    }
}
val Solution.hotReloadPluginModel get() = getOrCreateExtension("hotReloadPluginModel", ::HotReloadPluginModel)



/**
 * #### Generated from [HotReload.Extension.RiderModel.kt:15]
 */
data class MessageInfo (
    val title: String,
    val message: String
) : IPrintable {
    //companion
    
    companion object : IMarshaller<MessageInfo> {
        override val _type: KClass<MessageInfo> = MessageInfo::class
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MessageInfo  {
            val title = buffer.readString()
            val message = buffer.readString()
            return MessageInfo(title, message)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MessageInfo)  {
            buffer.writeString(value.title)
            buffer.writeString(value.message)
        }
        
        
    }
    //fields
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as MessageInfo
        
        if (title != other.title) return false
        if (message != other.message) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + title.hashCode()
        __r = __r*31 + message.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MessageInfo (")
        printer.indent {
            print("title = "); title.print(printer); println()
            print("message = "); message.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
}


/**
 * #### Generated from [HotReload.Extension.RiderModel.kt:10]
 */
data class SavedDocument (
    val filePath: String,
    val content: CharArray
) : IPrintable {
    //companion
    
    companion object : IMarshaller<SavedDocument> {
        override val _type: KClass<SavedDocument> = SavedDocument::class
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): SavedDocument  {
            val filePath = buffer.readString()
            val content = buffer.readCharArray()
            return SavedDocument(filePath, content)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: SavedDocument)  {
            buffer.writeString(value.filePath)
            buffer.writeCharArray(value.content)
        }
        
        
    }
    //fields
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as SavedDocument
        
        if (filePath != other.filePath) return false
        if (!(content contentEquals other.content)) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + filePath.hashCode()
        __r = __r*31 + content.contentHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("SavedDocument (")
        printer.indent {
            print("filePath = "); filePath.print(printer); println()
            print("content = "); content.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
}
