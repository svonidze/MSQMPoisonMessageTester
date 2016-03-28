using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Messaging;
using System.Windows.Forms;
using Cogin.Msmq;
using Message = System.Messaging.Message;

namespace Cogin
{
	public class PoisonMessageTester : Form
	{
		private Button receiveButton;
		private TextBox logTextBox;
		private System.Windows.Forms.Button sendOKButton;
		private System.Windows.Forms.Button sendPoisonButton;
		private System.Windows.Forms.Button refreshButton;
		private System.Windows.Forms.ListBox messagesList;
		private System.Windows.Forms.RadioButton alwaysRollbackRadioButton;
		private System.Windows.Forms.RadioButton sendToBackRadioButton;
		private System.Windows.Forms.GroupBox failureHandlerGroup;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		private System.Windows.Forms.ListBox queuesList;
		private System.Windows.Forms.RadioButton sendToDeadLetterRadioButton;
		private System.Windows.Forms.RadioButton separateRetryRadioButton;

		const string QUEUE_PATH = @".\private$\test_queue";
		const string RETRY_PATH = @".\private$\test_queue_retry";
		private System.Windows.Forms.RadioButton discardRadioButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		const string DEAD_LETTER_QUEUE_PATH = @".\private$\test_queue_dead_letter";

		public PoisonMessageTester()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Trace.Listeners.Add( new TextBoxTraceListener( logTextBox ) );
			queuesList.Items.Add( QUEUE_PATH );
			queuesList.Items.Add( RETRY_PATH );
			queuesList.Items.Add( DEAD_LETTER_QUEUE_PATH );
			queuesList.SelectedIndex = 0;
			reloadMessages();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.alwaysRollbackRadioButton = new System.Windows.Forms.RadioButton();
			this.receiveButton = new System.Windows.Forms.Button();
			this.sendOKButton = new System.Windows.Forms.Button();
			this.logTextBox = new System.Windows.Forms.TextBox();
			this.sendPoisonButton = new System.Windows.Forms.Button();
			this.messagesList = new System.Windows.Forms.ListBox();
			this.refreshButton = new System.Windows.Forms.Button();
			this.sendToBackRadioButton = new System.Windows.Forms.RadioButton();
			this.failureHandlerGroup = new System.Windows.Forms.GroupBox();
			this.separateRetryRadioButton = new System.Windows.Forms.RadioButton();
			this.sendToDeadLetterRadioButton = new System.Windows.Forms.RadioButton();
			this.discardRadioButton = new System.Windows.Forms.RadioButton();
			this.queuesList = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.failureHandlerGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// alwaysRollbackRadioButton
			// 
			this.alwaysRollbackRadioButton.Location = new System.Drawing.Point(16, 48);
			this.alwaysRollbackRadioButton.Name = "alwaysRollbackRadioButton";
			this.alwaysRollbackRadioButton.TabIndex = 2;
			this.alwaysRollbackRadioButton.Text = "Always roll back";
			// 
			// receiveButton
			// 
			this.receiveButton.Location = new System.Drawing.Point(16, 88);
			this.receiveButton.Name = "receiveButton";
			this.receiveButton.Size = new System.Drawing.Size(152, 32);
			this.receiveButton.TabIndex = 3;
			this.receiveButton.Text = "Receive message";
			this.receiveButton.Click += new System.EventHandler(this.receiveButton_Click);
			// 
			// sendOKButton
			// 
			this.sendOKButton.Location = new System.Drawing.Point(16, 8);
			this.sendOKButton.Name = "sendOKButton";
			this.sendOKButton.Size = new System.Drawing.Size(152, 32);
			this.sendOKButton.TabIndex = 4;
			this.sendOKButton.Text = "Send OK message";
			this.sendOKButton.Click += new System.EventHandler(this.sendOKButton_Click);
			// 
			// logTextBox
			// 
			this.logTextBox.Location = new System.Drawing.Point(16, 312);
			this.logTextBox.Multiline = true;
			this.logTextBox.Name = "logTextBox";
			this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.logTextBox.Size = new System.Drawing.Size(568, 120);
			this.logTextBox.TabIndex = 5;
			this.logTextBox.Text = "";
			// 
			// sendPoisonButton
			// 
			this.sendPoisonButton.Location = new System.Drawing.Point(16, 48);
			this.sendPoisonButton.Name = "sendPoisonButton";
			this.sendPoisonButton.Size = new System.Drawing.Size(152, 32);
			this.sendPoisonButton.TabIndex = 6;
			this.sendPoisonButton.Text = "Send poison message";
			this.sendPoisonButton.Click += new System.EventHandler(this.sendPoisonButton_Click);
			// 
			// messagesList
			// 
			this.messagesList.Location = new System.Drawing.Point(248, 112);
			this.messagesList.Name = "messagesList";
			this.messagesList.Size = new System.Drawing.Size(336, 173);
			this.messagesList.TabIndex = 7;
			// 
			// refreshButton
			// 
			this.refreshButton.Location = new System.Drawing.Point(504, 8);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(80, 32);
			this.refreshButton.TabIndex = 8;
			this.refreshButton.Text = "Refresh";
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// sendToBackRadioButton
			// 
			this.sendToBackRadioButton.Location = new System.Drawing.Point(16, 72);
			this.sendToBackRadioButton.Name = "sendToBackRadioButton";
			this.sendToBackRadioButton.TabIndex = 9;
			this.sendToBackRadioButton.Text = "Send to back";
			// 
			// failureHandlerGroup
			// 
			this.failureHandlerGroup.Controls.Add(this.separateRetryRadioButton);
			this.failureHandlerGroup.Controls.Add(this.sendToDeadLetterRadioButton);
			this.failureHandlerGroup.Controls.Add(this.sendToBackRadioButton);
			this.failureHandlerGroup.Controls.Add(this.alwaysRollbackRadioButton);
			this.failureHandlerGroup.Controls.Add(this.discardRadioButton);
			this.failureHandlerGroup.Location = new System.Drawing.Point(16, 136);
			this.failureHandlerGroup.Name = "failureHandlerGroup";
			this.failureHandlerGroup.Size = new System.Drawing.Size(216, 152);
			this.failureHandlerGroup.TabIndex = 10;
			this.failureHandlerGroup.TabStop = false;
			this.failureHandlerGroup.Text = "Failure handler";
			// 
			// separateRetryRadioButton
			// 
			this.separateRetryRadioButton.Location = new System.Drawing.Point(16, 120);
			this.separateRetryRadioButton.Name = "separateRetryRadioButton";
			this.separateRetryRadioButton.Size = new System.Drawing.Size(136, 24);
			this.separateRetryRadioButton.TabIndex = 11;
			this.separateRetryRadioButton.Text = "Separate retry queue";
			// 
			// sendToDeadLetterRadioButton
			// 
			this.sendToDeadLetterRadioButton.Location = new System.Drawing.Point(16, 96);
			this.sendToDeadLetterRadioButton.Name = "sendToDeadLetterRadioButton";
			this.sendToDeadLetterRadioButton.Size = new System.Drawing.Size(192, 24);
			this.sendToDeadLetterRadioButton.TabIndex = 10;
			this.sendToDeadLetterRadioButton.Text = "Retry, move to dead-letter queue";
			// 
			// discardRadioButton
			// 
			this.discardRadioButton.Checked = true;
			this.discardRadioButton.Location = new System.Drawing.Point(16, 24);
			this.discardRadioButton.Name = "discardRadioButton";
			this.discardRadioButton.Size = new System.Drawing.Size(136, 24);
			this.discardRadioButton.TabIndex = 12;
			this.discardRadioButton.TabStop = true;
			this.discardRadioButton.Text = "Discard";
			// 
			// queuesList
			// 
			this.queuesList.Location = new System.Drawing.Point(248, 24);
			this.queuesList.Name = "queuesList";
			this.queuesList.Size = new System.Drawing.Size(224, 56);
			this.queuesList.TabIndex = 12;
			this.queuesList.SelectedIndexChanged += new System.EventHandler(this.queuesList_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(248, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 16);
			this.label1.TabIndex = 13;
			this.label1.Text = "Queues:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(248, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 16);
			this.label2.TabIndex = 14;
			this.label2.Text = "Messages:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 296);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 16);
			this.label3.TabIndex = 15;
			this.label3.Text = "Log:";
			// 
			// PoisonMessageTester
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(600, 446);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.queuesList);
			this.Controls.Add(this.failureHandlerGroup);
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.messagesList);
			this.Controls.Add(this.sendPoisonButton);
			this.Controls.Add(this.logTextBox);
			this.Controls.Add(this.sendOKButton);
			this.Controls.Add(this.receiveButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "PoisonMessageTester";
			this.Text = "Poison message tester";
			this.failureHandlerGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new PoisonMessageTester());
		}

		private void sendOKButton_Click(object sender, System.EventArgs e)
		{
			sendMessage( "OK", "Message body" );
		}

		private void sendPoisonButton_Click(object sender, System.EventArgs e)
		{
			sendMessage( "Poison", "Poison message body" );
		}

		private void sendMessage( string label, string body )
		{
			try
			{
				Queue.Send( body, label, MessageQueueTransactionType.Single );
				Trace.WriteLine( "Sent message: " + label );
				reloadMessages();
			}
			catch ( Exception ex )
			{
				Trace.WriteLine( "Couldn't send message: " + ex );
			}
		}

		private void refreshButton_Click(object sender, System.EventArgs e)
		{
			reloadMessages();
		}

		private void reloadMessages()
		{
			messagesList.BeginUpdate();
			messagesList.Items.Clear();
			try
			{
				foreach( Message message in SelectedQueue.GetAllMessages() )
				{
					messagesList.Items.Add( message.Label + " - " + message.Id );
				}
			} catch( Exception ex )
			{
				Trace.WriteLine( "Couldn't refresh: " + ex );
			}
			messagesList.EndUpdate();
		}

		private void receiveButton_Click(object sender, System.EventArgs e)
		{
			IFailedMessageHandler failureHandler = null;
			if ( discardRadioButton.Checked )
			{
				failureHandler = new DiscardMessageHandler();
			}
			if ( alwaysRollbackRadioButton.Checked )
			{
				failureHandler = new AlwaysRollBackHandler();
			} else if ( sendToBackRadioButton.Checked )
			{
				failureHandler = new SendToBackHandler( 
						CreateAndGetQueue( DEAD_LETTER_QUEUE_PATH ) );
			} else if ( sendToDeadLetterRadioButton.Checked )
			{
				failureHandler = new RetrySendToDeadLetterQueueHandler( 
						CreateAndGetQueue( DEAD_LETTER_QUEUE_PATH ) );
			} else if ( separateRetryRadioButton.Checked )
			{
				failureHandler = new SeparateRetryQueueHandler( 
						CreateAndGetQueue( RETRY_PATH ) );
			}

			new MessageReceiver( Queue ).receiveMessage( failureHandler );
			reloadMessages();
		}

		private MessageQueue CreateAndGetQueue( string path )
		{
			if ( ! MessageQueue.Exists( path ) )
			{
				MessageQueue.Create( path, true );
			}
			return new MessageQueue( path );
		}

		private void queuesList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			reloadMessages();
		}

		private MessageQueue SelectedQueue
		{
			get { return CreateAndGetQueue( ( string ) queuesList.SelectedItem ); }
		}

		private MessageQueue Queue
		{
			get
			{
				return CreateAndGetQueue( QUEUE_PATH );
			}
		}
	}

	public class TextBoxTraceListener : TraceListener
	{
		TextBox textBox;
		ArrayList lines = new ArrayList();

		public TextBoxTraceListener( TextBox textBox )
		{
			this.textBox = textBox;
		}

		public override void Write( string message )
		{
			WriteLine( message );
		}

		public override void WriteLine( string message )
		{
			lines.Add( message );
			if ( lines.Count > 100 )
			{
				lines.RemoveAt( 0 );
			}
			textBox.Lines = (string[]) lines.ToArray( typeof(string) );
			textBox.SelectionLength = textBox.Text.Length;
			textBox.ScrollToCaret();
		}
	}
}
