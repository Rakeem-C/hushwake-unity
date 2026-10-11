using UnityEngine;
namespace Hushwake {
 public static class FrameDiagnostics {
  public static bool visible;public static float fps,script,visibility,render,ui,actors;public static int pieces,active;
  public static long Stamp()=>System.Diagnostics.Stopwatch.GetTimestamp();
  public static float Milliseconds(long stamp)=>(float)((Stamp()-stamp)*1000.0/System.Diagnostics.Stopwatch.Frequency);
  public static void Draw(){if(!visible)return;GUI.Box(new Rect(20,125,430,145),"");GUI.Label(new Rect(32,135,410,130),$"Frame diagnostics (F3 to close)\n{fps:F1} fps | render submission {render:F1} ms\nGame update {script:F1} ms | UI {ui:F1} ms\nVisibility {visibility:F1} ms | actors {actors:F1} ms\nDetail pieces: {active} active / {pieces}",new GUIStyle(GUI.skin.label){fontSize=14,normal={textColor=Color.white}});}
 }
}
