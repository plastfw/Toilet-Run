using Agava.YandexGames;
using Agava.YandexMetrica;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelHandler : MonoBehaviour
{
  private const int LastLevelIndex = 69;

  [SerializeField] private List<Level> _levels;
  [SerializeField] private GameStateHandler _game;
  [SerializeField] private UIHandler _ui;

  private Transform _levelPool;
  private Level _currentLevel;
  private int _currentLevelIndex;
  private float _delayBeforeOpenFailPanel = 0.4f;

  public List<Unit> Units => _currentLevel.Units;
  public int CountLevels => _levels.Count;

  private void OnEnable()
  {
    _ui.BackwardClicked += DestroyLevel;
    _ui.RestartClicked += RestartLevel;
    _ui.NextClicked += RaiseLevel;
  }

  private void OnDisable()
  {
    _ui.BackwardClicked -= DestroyLevel;
    _ui.RestartClicked -= RestartLevel;
    _ui.NextClicked -= RaiseLevel;
  }

  private void Awake()
  {
    _levelPool = GetComponentInChildren<Transform>();
    _currentLevelIndex = LevelIndexSaver.LoadLevel();
  }

  public void ShowGuides() => _currentLevel.ShowGuides();

  public void SpawnCurrentLevel()
  {
    _currentLevel = Instantiate(_levels[LevelIndexSaver.LoadLevel()], _levelPool);
    Subscribe(_currentLevel);
    _game.SetUnitsList(_currentLevel.Units);
    _game.SetEnemiesList(_currentLevel.Enemies);
  }

  private void RestartLevel()
  {
    DestroyLevel();
    SpawnCurrentLevel();
    _ui.ChangeCompletePanelState(false);
    _ui.LoseLevelPanel.Close();
  }

  private void OnLevelComplete()
  {
    if (LevelIndexSaver.LoadLevel() == LastLevelIndex)
      _ui.ShowFinalPanel();
    else
      _ui.ChangeCompletePanelState(true);

#if UNITY_EDITOR
    return;
#endif
    string levelCompleteText = $"{_currentLevel.gameObject.name}Complete";
    string totalLevelCompleteText = levelCompleteText.Replace("(Clone)", "");
    YandexMetrica.Send(totalLevelCompleteText);
  }

  private void OnLevelFail()
  {
    StartCoroutine(OpenFailPanelWithDelay());

#if UNITY_EDITOR
    return;
#endif
    string levelFailText = $"{_currentLevel.gameObject.name}Fail";
    string totallevelFailText = levelFailText.Replace("(Clone)", "");
    YandexMetrica.Send(totallevelFailText);
  }

  private void RaiseLevel()
  {
    if (_currentLevelIndex == LastLevelIndex)
    {
      _currentLevelIndex = LevelIndexSaver.LoadLevel();
      DestroyLevel();
      return;
    }

    _currentLevelIndex = LevelIndexSaver.LoadLevel() + 1;
    LevelIndexSaver.SaveLevel(_currentLevelIndex);
    _ui.RaiseLevelCounter();
    DestroyLevel();

    SpawnCurrentLevel();
    _ui.ChangeCompletePanelState(false);
#if UNITY_EDITOR
    return;
#endif
    if (PlayerAccount.IsAuthorized && PlayerAccount.HasPersonalProfileDataPermission)
    {
      Leaderboard.SetScore(YandexLeaderboard.LeaderboardName, ES3.Load(LevelIndexSaver.Level, 0));
    }
  }

  private void DestroyLevel()
  {
    _game.DestroyAllLines();
    UnSubscribe(_currentLevel);
    Destroy(_currentLevel.gameObject);
  }

  private void UnSubscribe(Level level)
  {
    level.LevelComplete -= OnLevelComplete;
    level.LevelFail -= OnLevelFail;
  }

  private void Subscribe(Level level)
  {
    level.LevelComplete += OnLevelComplete;
    level.LevelFail += OnLevelFail;
  }


  private IEnumerator OpenFailPanelWithDelay()
  {
    var delay = new WaitForSeconds(_delayBeforeOpenFailPanel);

    yield return delay;

    _ui.LoseLevelPanel.Open();
  }
}