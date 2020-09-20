package hotreload.extension.rider.services

import com.intellij.ide.actions.SaveAllAction
import com.intellij.ide.actions.SaveDocumentAction
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.DataContext
import com.intellij.openapi.actionSystem.ex.AnActionListener
import com.intellij.openapi.project.Project
import com.jetbrains.rd.util.reactive.hasTrueValue
import com.jetbrains.rider.model.SavedDocument
import com.jetbrains.rider.model.hotReloadPluginModel
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.util.idea.PsiFile

class SaveDocumentListener(val project: Project) : AnActionListener {

    override fun afterActionPerformed(action: AnAction, dataContext: DataContext, event: AnActionEvent) {
        super.afterActionPerformed(action, dataContext, event)

        if (action is SaveDocumentAction || action is SaveAllAction) {
            val psiFile = event.dataContext.PsiFile ?: return
            val filePath = psiFile.containingFile.viewProvider.virtualFile.canonicalPath
            val content = psiFile.containingFile.textToCharArray()

            val document = SavedDocument(filePath.toString(), content)
            val model = project.solution.hotReloadPluginModel
            if (model.isEnabled.hasTrueValue) {
                model.reload.fire(document)
            }
        }
    }
}

