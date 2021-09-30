package hotreload.extension.rider.component

import com.intellij.notification.*
import com.intellij.notification.impl.NotificationsManagerImpl
import com.intellij.openapi.project.Project
import com.jetbrains.rd.ide.model.MessageInfo
import com.jetbrains.rd.ide.model.hotReloadPluginModel
import com.jetbrains.rdclient.util.idea.LifetimedProjectComponent
import com.jetbrains.rider.projectView.solution

class ShowNotificationComponent(project: Project) : LifetimedProjectComponent(project) {

    companion object {
    }

    init {
        val model = project.solution.hotReloadPluginModel
        model.showMessage.advise(componentLifetime) {
            showNotification(it)
        }
    }

    private fun showNotification(messageInfo: MessageInfo) {
        val groupId = "Hot Reload Group"
        val yamlNotification = Notification(groupId, messageInfo.title, messageInfo.message, NotificationType.INFORMATION)
        NotificationGroupManager.getInstance().getNotificationGroup(groupId)
                .createNotification(yamlNotification.content, NotificationType.INFORMATION)
                .notify(project);
    }
}