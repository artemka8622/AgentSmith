using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

#if RESHARPER20172
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.Application.UI.Actions;
#else
using JetBrains.UI.ActionsRevised;
using JetBrains.ActionManagement;
#endif

namespace AgentSmith.Comments.Reflow
{
	//[ActionHandler(new[] { "AgentSmith.Reflow" })]
	[Action("AgentSmith.Reflow")]
    public class ReflowMenuAction : IExecutableAction
    {
        #region Implementation of IActionHandler

        private IProjectFile GetProjectFile(IDataContext context)
        {
            IProjectModelElement element = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
            IProjectFile file = element as IProjectFile;
            if (file == null) return null;
            
            if (!file.LanguageType.Equals(CSharpProjectFileType.Instance)) return null;
            return file;
        }

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            if (GetProjectFile(context) == null) return nextUpdate();
            if (context.GetData(ProjectModelDataConstants.SOLUTION) == null) return nextUpdate();
            return true;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            IProjectFile projectFile = GetProjectFile(context);
            if (projectFile == null) return;

            IPsiSourceFile sourceFile = projectFile.ToSourceFile();
            if (sourceFile == null) return;
			IFile file = sourceFile.GetTheOnlyPsiFile(CSharpLanguage.Instance);
            if (file == null) return;

	        file.GetPsiServices()
	            .Transactions.Execute(
		            "Reflow XML Documentation Comments",
		            () => {
			            using (WriteLockCookie.Create()) {
				            foreach (var docCommentBlockOwner in file.Descendants<IDocCommentBlockOwner>()) {
					            CommentReflowAction.ReFlowCommentBlockNode(
						            docCommentBlockOwner.GetSolution(),
						            null,
						            docCommentBlockOwner.DocCommentBlock);
				            }
			            }
		            });
        }

        #endregion
    }
}