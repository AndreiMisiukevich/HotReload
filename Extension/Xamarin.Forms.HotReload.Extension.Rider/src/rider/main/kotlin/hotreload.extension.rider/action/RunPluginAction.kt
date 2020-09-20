package hotreload.extension.rider.action

import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.jetbrains.rd.util.reactive.IOptProperty
import com.jetbrains.rd.util.reactive.valueOrDefault
import com.jetbrains.rider.model.hotReloadPluginModel
import com.jetbrains.rider.projectView.solution
import hotreload.extension.rider.icons.HotReloadIcons
import javax.swing.Icon

class RunPluginAction(icon: Icon) : AnAction("Enable HotReload", null, icon) {

    constructor(): this(HotReloadIcons.IconOff)

    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project ?: return

        val model = project.solution.hotReloadPluginModel
        val isEnabled = model.isEnabled
        updateIcon(e, isEnabled)
        model.enable.fire(isEnabled.valueOrDefault(false))
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
