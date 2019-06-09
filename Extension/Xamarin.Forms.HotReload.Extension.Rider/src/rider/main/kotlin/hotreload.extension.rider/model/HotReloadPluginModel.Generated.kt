@file:Suppress("PackageDirectoryMismatch", "UnusedImport", "unused", "LocalVariableName")
package com.jetbrains.rider.model

import com.jetbrains.rd.framework.*
import com.jetbrains.rd.framework.base.*
import com.jetbrains.rd.framework.impl.*

import com.jetbrains.rd.util.lifetime.*
import com.jetbrains.rd.util.reactive.*
import com.jetbrains.rd.util.string.*
import com.jetbrains.rd.util.*
import kotlin.reflect.KClass



class HotReloadPluginModel private constructor(
    private val _reload: RdSignal<SavedDocument>,
    private val _isEnabled: RdOptionalProperty<Boolean>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers) {
            serializers.register(SavedDocument)
        }
        
        
        
        
        const val serializationHash = 4156314565325463560L
    }
    override val serializersOwner: ISerializersOwner get() = HotReloadPluginModel
    override val serializationHash: Long get() = HotReloadPluginModel.serializationHash
    
    //fields
    val reload: ISignal<SavedDocument> get() = _reload
    val isEnabled: IOptProperty<Boolean> get() = _isEnabled
    //initializer
    init {
        _isEnabled.optimizeNested = true
    }
    
    init {
        bindableChildren.add("reload" to _reload)
        bindableChildren.add("isEnabled" to _isEnabled)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdSignal<SavedDocument>(SavedDocument),
        RdOptionalProperty<Boolean>(FrameworkMarshallers.Bool)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter) {
        printer.println("HotReloadPluginModel (")
        printer.indent {
            print("reload = "); _reload.print(printer); println()
            print("isEnabled = "); _isEnabled.print(printer); println()
        }
        printer.print(")")
    }
}
val Solution.hotReloadPluginModel get() = getOrCreateExtension("hotReloadPluginModel", ::HotReloadPluginModel)



data class SavedDocument (
    val filePath: String,
    val content: CharArray
) : IPrintable {
    //companion
    
    companion object : IMarshaller<SavedDocument> {
        override val _type: KClass<SavedDocument> = SavedDocument::class
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): SavedDocument {
            val filePath = buffer.readString()
            val content = buffer.readCharArray()
            return SavedDocument(filePath, content)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: SavedDocument) {
            buffer.writeString(value.filePath)
            buffer.writeCharArray(value.content)
        }
        
    }
    //fields
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as SavedDocument
        
        if (filePath != other.filePath) return false
        if (!(content contentEquals other.content)) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int {
        var __r = 0
        __r = __r*31 + filePath.hashCode()
        __r = __r*31 + content.contentHashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter) {
        printer.println("SavedDocument (")
        printer.indent {
            print("filePath = "); filePath.print(printer); println()
            print("content = "); content.print(printer); println()
        }
        printer.print(")")
    }
}
