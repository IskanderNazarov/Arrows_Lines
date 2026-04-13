using System;
using UnityEngine;

public abstract class ToggleGroup<T> : MonoBehaviour where T : ToggleButton {
    [SerializeField] private T[] buttons;
    [SerializeField] private bool canHaveNonSelectedButtons = true;

    public T[] ToggleButtons => buttons;


    public Action<ToggleButton> OnButtonSelected;
    public Action<ToggleButton> OnButtonDeselected;

    public void Init(int initialSelectedButtonIndex = -1) {
        for (var i = 0; i < buttons.Length; i++) {
            var b = buttons[i];
            b.init(i);
            b.onClickedCallback += OnButtonClicked;
            b.onSelected += OnSelectedClicked;
            b.onDeselected += OnDeselectedClicked;
        }

        Setup();

        ResetToDeselectedState();
        if (initialSelectedButtonIndex >= 0 && initialSelectedButtonIndex < buttons.Length) {
            buttons[initialSelectedButtonIndex].Select();
        }
    }

    protected abstract void Setup();

    private void OnButtonClicked(ToggleButton b) {
        if (b.isSelected) {
            if (canHaveNonSelectedButtons) {
                b.Deselect();
            }

            return;
        }

        foreach (var btn in buttons) {
            if (btn.isSelected) {
                btn.Deselect();
                break;
            }
        }


        b.Select();
    }

    private void OnSelectedClicked(ToggleButton b) {
        OnButtonSelected?.Invoke(b);
    }

    private void OnDeselectedClicked(ToggleButton b) {
        OnButtonDeselected?.Invoke(b);
    }

    public void EnableInteraction(bool enable) {
        foreach (var b in buttons) {
            b.EnableInteraction(enable);
        }
    }

    public void ResetToDeselectedState() {
        foreach (var b in buttons) {
            b.SetToDeselectedState();
        }
    }

    public void SelectButton(int index) {
        buttons[index].Select();
    }
}