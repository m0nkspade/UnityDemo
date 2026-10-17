using Unity.CodeEditor;

  public static class SyncProject
  {
      public static void Run()
      {
          CodeEditor.CurrentEditor.SyncAll();
      }
  }