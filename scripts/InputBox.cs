using Godot;
using System;

public partial class InputBox : AcceptDialog
{
	private LineEdit line_edit;
	private Label tip_label;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tip_label = this.GetChild(0).GetChild<Label>(0);
		line_edit = this.GetChild(0).GetChild<LineEdit>(1);
	}
	public void SetHolderText(string holder_text)
	{
		line_edit.PlaceholderText= holder_text;
	}
	public void SetLineEditText(string text)
	{
		line_edit.Text= text;
	}
	public void SetTipText(string text)
	{
		tip_label.Text= text;
	}
	public string GetValue()
	{
		return line_edit.Text;
	}
}
