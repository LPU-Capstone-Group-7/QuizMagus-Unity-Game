using UnityEngine;

public class CrossWordManager : MonoBehaviour
{
  
  private CrossWordGridManager gridManager;
  private CrossWordSettings crossWordSettings;
  
  private void Start()
  {
    crossWordSettings = DataManager.instance.GetGameSettings<CrossWordSettings>();

    gridManager = CrossWordGridManager.instance;
    gridManager.CreateCrossWordGrid(crossWordSettings.triviaQuestions);
    
  }
  
}