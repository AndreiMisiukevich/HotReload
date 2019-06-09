package hotreload.extension.rider.action

import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.jetbrains.rd.util.reactive.IOptProperty
import com.jetbrains.rider.model.hotReloadPluginModel
import hotreload.extension.rider.icons.HotReloadIcons

import javax.swing.*

import com.jetbrains.rider.projectView.solution

class RunPluginAction(icon: Icon) : AnAction("Enable HotReload", "", icon) {

    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project?: return

        val isEnabled = project.solution.hotReloadPluginModel.isEnabled
        updateIcon(e, isEnabled)
    }

    fun updateIcon(e: AnActionEvent, isEnabled: IOptProperty<Boolean>?) {
        val valueOrNull = isEnabled?.valueOrNull
        if (valueOrNull == null) {
            isEnabled?.set(false)
        }
        val presentation = e.presentation
        if (!isEnabled!!.valueOrNull!!) {
            presentation.setIcon(HotReloadIcons.IconOn)
            presentation.text = "Disable HotReload"
            isEnabled.set(true)
        } else {
            presentation.setIcon(HotReloadIcons.IconOff)
            presentation.text = "Enable HotReload"
            isEnabled.set(false)
        }
    }
}
