package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.*
import com.jetbrains.rider.model.nova.ide.SolutionModel

@Suppress("unused")
object HotReloadPluginModel : Ext(SolutionModel.Solution) {

    val savedDocument = structdef {
        field("filePath", string)
        field("content", array(char))
    }

    val messageInfo = structdef {
        field("title", string)
        field("message", string)
    }

    init {
        source("reload", savedDocument)

        source("enable", bool)
        property("isEnabled", bool)

        sink("showMessage", messageInfo)
    }
}