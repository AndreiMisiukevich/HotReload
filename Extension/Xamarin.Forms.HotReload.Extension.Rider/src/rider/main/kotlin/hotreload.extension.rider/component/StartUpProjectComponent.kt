package hotreload.extension.rider.component

import com.intellij.openapi.Disposable
import com.intellij.openapi.actionSystem.ActionManager
import com.intellij.openapi.actionSystem.DefaultActionGroup
import com.intellij.openapi.actionSystem.impl.ActionToolbarImpl
import com.intellij.openapi.components.ProjectComponent
import hotreload.extension.rider.action.RunPluginAction
import hotreload.extension.rider.icons.HotReloadIcons


class StartUpProjectComponent: Disposable {
    override fun dispose() {
        val actionManager = ActionManager.getInstance()
        actionManager.unregisterAction("HotReloadAction")    }

    init {
        val actionManager = ActionManager.getInstance()
        val hotReloadAction = RunPluginAction(HotReloadIcons.IconOff)
        actionManager.registerAction("HotReloadAction", hotReloadAction)

        // Gets an instance of the WindowMenu action group.
        val toolbar = actionManager.getAction("NavBarToolBar") as DefaultActionGroup

        // Adds a separator and a new menu command to the WindowMenu group on the main menu.
        toolbar.addSeparator()
        toolbar.add(hotReloadAction)
        ActionToolbarImpl.updateAllToolbarsImmediately()
    }
}