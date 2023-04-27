mergeInto(LibraryManager.library, {
  SendReactGame: function (message) {
  window.dispatchReactUnityEvent("SendReactGame",UTF8ToString(message));
  },
  SendTriviaGameResult : function (triviaGameResultJSON){
    window.dispatchReactUnityEvent("SendTriviaGameResult", UTF8ToString(triviaGameResultJSON))
  }
});