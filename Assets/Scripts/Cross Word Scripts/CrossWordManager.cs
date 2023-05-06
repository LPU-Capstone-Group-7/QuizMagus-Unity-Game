using UnityEngine;

public class CrossWordManager : MonoBehaviour
{
  
  private CrossWordGridManager gridManager;
  private CrossWordSettings crossWordSettings;
  
  private void Start()
  {
    gridManager = CrossWordGridManager.instance;
    
    crossWordSettings = DataManager.instance.GetGameSettings<CrossWordSettings>();
    
  }
  
}