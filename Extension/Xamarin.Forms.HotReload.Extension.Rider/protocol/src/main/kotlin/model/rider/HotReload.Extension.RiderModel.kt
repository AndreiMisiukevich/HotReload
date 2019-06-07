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

    init {
        source("reload", savedDocument)

        property("isEnabled", bool)
    }
}