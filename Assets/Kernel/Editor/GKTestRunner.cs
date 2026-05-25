// =============================================================================
//  GKTestRunner.cs
//  One-click / hotkey test runner for GoldbergKernel tests.
//  Menu:    MNet > Run Kernel Tests
//  Hotkey:  Ctrl + Shift + T
// =============================================================================

using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace MachineNet.Editor
{
    public static class GKTestRunner
    {
        [MenuItem("MNet/Run Kernel Tests  %#t")]
        public static void RunAll()
        {
            var api    = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter {
                testMode      = TestMode.EditMode,
                assemblyNames = new[] { "MachineNet.Kernel.Tests" }
            };
            api.Execute(new ExecutionSettings(filter));
            Debug.Log("[MNet] Kernel tests fired. Watch Test Runner window.");
        }
    }
}
