using UnityEngine;

namespace AudioSDK
{
    public static class AudioControllerHelper
    {
        public static AudioSubItem[] _ChooseSubItems(AudioItem audioItem, AudioObject useExistingAudioObj)
        {
            return _ChooseSubItems(audioItem, audioItem.SubItemPickMode, useExistingAudioObj);
        }

        public static AudioSubItem _ChooseSingleSubItem(AudioItem audioItem, AudioPickSubItemMode pickMode, AudioObject useExistingAudioObj)
        {
            return _ChooseSubItems(audioItem, pickMode, useExistingAudioObj)[0];
        }

        public static AudioSubItem _ChooseSingleSubItem(AudioItem audioItem) { return _ChooseSingleSubItem(audioItem, audioItem.SubItemPickMode, null); }

        private static AudioSubItem[] _ChooseSubItems(AudioItem audioItem, AudioPickSubItemMode pickMode, AudioObject useExistingAudioObj)
        {
            if(audioItem.SubItems == null)
                return null;
            var length = audioItem.SubItems.Length;
            if(length == 0)
                return null;
            var index1 = 0;
            var flag = useExistingAudioObj != null;
            var lastChosen = !flag ? audioItem.LastChosen : useExistingAudioObj.LastChosenSubItemIndex;
            if(length > 1)
                switch(pickMode)
                {
                    case AudioPickSubItemMode.Disabled:
                        return null;
                    case AudioPickSubItemMode.Random:
                        index1 = _ChooseRandomSubitem(audioItem, true, lastChosen);
                        break;
                    case AudioPickSubItemMode.RandomNotSameTwice:
                        index1 = _ChooseRandomSubitem(audioItem, false, lastChosen);
                        break;
                    case AudioPickSubItemMode.Sequence:
                        index1 = (lastChosen + 1) % length;
                        break;
                    case AudioPickSubItemMode.SequenceWithRandomStart:
                        index1 = lastChosen != -1 ? (lastChosen + 1) % length : Random.Range(0, length);
                        break;
                    case AudioPickSubItemMode.AllSimultaneously:
                        var audioSubItemArray = new AudioSubItem[length];
                        for(var index2 = 0; index2 < length; ++index2)
                            audioSubItemArray[index2] = audioItem.SubItems[index2];
                        return audioSubItemArray;
                    case AudioPickSubItemMode.TwoSimultaneously:
                        return new[]
                        {
                            _ChooseSingleSubItem(audioItem, AudioPickSubItemMode.RandomNotSameTwice, useExistingAudioObj),
                            _ChooseSingleSubItem(audioItem, AudioPickSubItemMode.RandomNotSameTwice, useExistingAudioObj)
                        };
                    case AudioPickSubItemMode.StartLoopSequenceWithFirst:
                        index1 = !flag ? 0 : (lastChosen + 1) % length;
                        break;
                    case AudioPickSubItemMode.RandomNotSameTwiceOddsEvens:
                        index1 = _ChooseRandomSubitem(audioItem, false, lastChosen, true);
                        break;
                }
            if(flag)
                useExistingAudioObj.LastChosenSubItemIndex = index1;
            else
                audioItem.LastChosen = index1;
            return new[] { audioItem.SubItems[index1] };
        }

        private static int _ChooseRandomSubitem(AudioItem audioItem, bool allowSameElementTwiceInRow, int lastChosen, bool switchOddsEvens = false)
        {
            var length = audioItem.SubItems.Length;
            var num1 = 0;
            var num2 = 0.0f;
            float max;
            if(!allowSameElementTwiceInRow)
            {
                if(lastChosen >= 0)
                {
                    num2 = audioItem.SubItems[lastChosen].SummedProbability;
                    if(lastChosen >= 1)
                        num2 -= audioItem.SubItems[lastChosen - 1].SummedProbability;
                }
                else
                {
                    num2 = 0.0f;
                }
                max = 1f - num2;
            }
            else
            {
                max = 1f;
            }
            var num3 = Random.Range(0.0f, max);
            int i;
            for(i = 0; i < length - 1; ++i)
            {
                var summedProbability = audioItem.SubItems[i].SummedProbability;
                if(!switchOddsEvens || IsOdd(i) != IsOdd(lastChosen))
                {
                    if(!allowSameElementTwiceInRow)
                        if(i != lastChosen || summedProbability == 1.0 && audioItem.SubItems[i].DisableOtherSubitems)
                        {
                            if(i > lastChosen)
                                summedProbability -= num2;
                        }
                        else
                        {
                            continue;
                        }
                    if(summedProbability > (double)num3)
                    {
                        num1 = i;
                        break;
                    }
                }
            }
            if(i == length - 1)
                num1 = length - 1;
            return num1;
        }

        private static bool IsOdd(int i) { return (uint)(i % 2) > 0U; }
    }
}
