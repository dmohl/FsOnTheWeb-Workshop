
[<AutoOpen>]
module Show

open System.Windows.Forms
open System.Drawing

let form = 
    new Form(Visible = true, 
             Text = "A Simple F# Form", 
             TopMost = true, 
             Size = Size(600,600))

let textBox = 
    new RichTextBox(Dock = DockStyle.Fill, 
                    Text = "F# Programming is Fun!",
                    Font = new Font("Lucida Console",16.0f,FontStyle.Bold),
                    ForeColor = Color.DarkBlue)

form.Controls.Add(textBox)

let show x = 
   textBox.Text <- sprintf "%40A" x
   Application.DoEvents()
