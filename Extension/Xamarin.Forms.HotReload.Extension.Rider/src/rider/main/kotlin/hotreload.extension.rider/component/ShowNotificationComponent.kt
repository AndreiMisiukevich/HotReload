package hotreload.extension.rider.component

import com.intellij.notification.Notification
import com.intellij.notification.NotificationGroup
import com.intellij.notification.NotificationType
import com.intellij.notification.Notifications
import com.intellij.openapi.project.Project
import com.jetbrains.rdclient.util.idea.LifetimedProjectComponent
import com.jetbrains.rider.model.MessageInfo
import com.jetbrains.rider.model.hotReloadPluginModel
import com.jetbrains.rider.projectView.solution

class ShowNotificationComponent(project: Project) : LifetimedProjectComponent(project) {

    companion object {
        private val notificationGroupId = NotificationGroup.balloonGroup("Hot ReloadEnable")
    }

    init {
        val model = project.solution.hotReloadPluginModel
        model.showMessage.advise(componentLifetime){
            showNotification(it)
        }
    }

    private fun showNotification(messageInfo: MessageInfo) {
        val yamlNotification = Notification(notificationGroupId.displayId, messageInfo.title, messageInfo.message, NotificationType.INFORMATION)
        Notifications.Bus.notify(yamlNotification, project)
    }
}