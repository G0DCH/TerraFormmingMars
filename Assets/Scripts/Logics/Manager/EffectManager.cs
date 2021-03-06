﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TerraFormmingMars.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace TerraFormmingMars.Logics.Manager
{
    public class EffectManager : Utility.Singleton<EffectManager>
    {
        [SerializeField]
        private GameObject selectWindow;

        [SerializeField]
        private GameObject selectTarget;

        //Effect, TargetEffect 통합 실행
        public void ExecuteCard(List<FunctionData> functionDatas, List<FunctionData> targetFunctionDatas)
        {
            // 타깃 지정이 있다면 일반 효과는 지정 할 때 까지 대기
            if (targetFunctionDatas.Count > 0)
            {
                ExecuteTargetEffect(targetFunctionDatas);
                StartCoroutine(Execute_Effect(functionDatas, true));
            }
            // 아니라면 그냥 실행
            else
            {
                ExecuteEffect(functionDatas);
            }
        }

        public void ExecuteEffect(List<FunctionData> functionDatas)
        {
            StartCoroutine(Execute_Effect(functionDatas));
        }

        public void ExecuteTargetEffect(List<FunctionData> functionDatas)
        {
            List<object> arguments = new List<object>();

            List <FunctionData> tmpList = new List<FunctionData>(functionDatas);
            FunctionData targetFunction = tmpList[0];
            tmpList.RemoveAt(0);

            arguments.Clear();
            arguments.AddRange(targetFunction.FunctionArguments);

            Type type = GetType();
            MethodInfo functionInfo =
                type.GetMethod(targetFunction.FunctionName, BindingFlags.Instance | BindingFlags.NonPublic);
            functionInfo.Invoke(this, arguments.ToArray());

            PlayerManager.Instance.TargetPlayer = null;
            StartCoroutine(Execute_TargetEffect(tmpList));
        }

        private void ChangeSourceProduct(string sourceType, string _sourceDifference, Player player = null)
        {
            if (player == null)
            {
                player = PlayerManager.Instance.TurnPlayer;
            }

            int sourceDifference = int.Parse(_sourceDifference);

            Source source;
            //자원을 알맞게 획득했다면 생산량 변경
            if (player.StringSourceMap.TryGetValue(sourceType, out source) == true)
            {
                source.Product += sourceDifference;
            }
            else
            {
                Debug.LogError(nameof(ChangeSourceProduct) + " : " + sourceType + "에 해당하는 자원이 존재하지 않습니다.");
            }
        }

        private void ChangeSourceAmount(string sourceType, string _sourceDifference, Player player = null)
        {
            if (player == null)
            {
                player = PlayerManager.Instance.TurnPlayer;
            }

            int sourceDifference = int.Parse(_sourceDifference);

            Source source;
            //자원을 알맞게 획득했다면 생산량 변경
            if (player.StringSourceMap.TryGetValue(sourceType, out source) == true)
            {
                source.Amount += sourceDifference;
            }
            else
            {
                Debug.LogError(nameof(ChangeSourceAmount) + " : " + sourceType + "에 해당하는 자원이 존재하지 않습니다.");
            }
        }

        //SelectWindow에 들어갈 유저를 조건에 맞게 필터링 후 추가
        private void FilteringTargetUser(string sourceType, string which)
        {
            foreach (Player player in PlayerManager.Instance.PlayerList)
            {
                if (player.StringSourceMap.TryGetValue(sourceType, out Source source) == true)
                {

                    if (which == "Product")
                    {
                        if (source.Product > 0)
                        {
                            AddTarget(selectTarget, player, source);
                        }
                    }
                    else if (which == "Amount")
                    {
                        if (source.Amount > 0)
                        {
                            AddTarget(selectTarget, player, source);
                        }
                    }
                }
                else
                {
                    Debug.LogError(nameof(FilteringTargetUser) + " : " + sourceType + "에 해당하는 자원이 없습니다.");
                }
            }

            selectWindow.SetActive(true);
        }

        private void AddTarget(GameObject target, Player player, Source source)
        {
            GameObject addedTarget = Instantiate(selectTarget, selectWindow.transform);

            if (addedTarget == null)
            {
                Debug.LogError(nameof(AddTarget) + " : 타겟 추가에 실패했습니다.");
                return;
            }
            else
            {
                Toggle toggle = addedTarget.GetComponent<Toggle>();
                toggle.group = selectWindow.GetComponent<ToggleGroup>();

                toggle.onValueChanged.AddListener((value) => { UIManager.Instance.SetTmpSelectedPlayer(value, player); });

                string labelName = "Label";
                Text label = addedTarget.transform.Find(labelName).GetComponent<Text>();

                label.text = string.Format("{0} [{1} {2}(+{3})]", player.playerName, source.SourceType, source.Amount, source.Product);
            }
        }

        private IEnumerator Execute_TargetEffect(List<FunctionData> functionDatas)
        {
            while (PlayerManager.Instance.TargetPlayer == null)
            {
                yield return new WaitForEndOfFrame();
            }

            List<object> arguments = new List<object>();

            foreach (FunctionData functionData in functionDatas)
            {
                arguments.Clear();
                arguments.AddRange(functionData.FunctionArguments);
                arguments.Add(PlayerManager.Instance.TargetPlayer);

                Type type = GetType();
                MethodInfo functionInfo =
                    type.GetMethod(functionData.FunctionName, BindingFlags.Instance | BindingFlags.NonPublic);
                functionInfo.Invoke(this, arguments.ToArray());
            }

            yield break;
        }

        private IEnumerator Execute_Effect(List<FunctionData> functionDatas, bool isWaiting = false)
        {
            // 타깃 선택 때문에 기다려야 한다면 대기
            while (isWaiting == true && PlayerManager.Instance.TargetPlayer == null)
            {
                yield return new WaitForEndOfFrame();
            }

            List<object> arguments = new List<object>();
            foreach (FunctionData functionData in functionDatas)
            {
                arguments.Clear();
                arguments.AddRange(functionData.FunctionArguments);
                arguments.Add(PlayerManager.Instance.TurnPlayer);

                Type type = GetType();
                MethodInfo functionInfo =
                    type.GetMethod(functionData.FunctionName, BindingFlags.Instance | BindingFlags.NonPublic);
                functionInfo.Invoke(this, arguments.ToArray());
            }

            yield break;
        }
    }
}