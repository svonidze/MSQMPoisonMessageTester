using System;
using System.Diagnostics;
using System.Messaging;
using System.Threading;

namespace Cogin.Msmq
{
	public class MessageReceiver 
	{
		protected MessageQueue incomingQueue;

		public MessageReceiver( MessageQueue incomingQueue )
		{
			this.incomingQueue = incomingQueue;
			incomingQueue.MessageReadPropertyFilter.SetAll();
		}

		public void receiveMessage( IFailedMessageHandler failedMessageHandler )
		{
			MessageQueueTransaction transaction = new MessageQueueTransaction();
			transaction.Begin();
			Message message = null;
			try
			{
				message = incomingQueue.Receive( TimeSpan.Zero, transaction );
				Trace.WriteLine( "Received message ID: " + message.Id );
				processMessage( message );
				transaction.Commit();
				Trace.WriteLine( "Message processed OK" );
			}
			catch (Exception e)
			{
				Trace.WriteLine( "Message failed" );
				TransactionAction transactionAction = TransactionAction.ROLLBACK;
				
				if ( message == null )
				{
					Trace.WriteLine( "Message couldn't be received: " + e.Message );
				} 
				else 
				{
					try
					{
						transactionAction = failedMessageHandler.HandleFailedMessage( message, transaction );
					} catch( Exception failureHandlerException )
					{
						Trace.WriteLine( "Error during failure handling: " + failureHandlerException.Message );
					}
				}

				if ( transactionAction == TransactionAction.ROLLBACK ) 
				{
					transaction.Abort();
					Trace.WriteLine( "Transaction rolled back" );
				} 
				else
				{
					transaction.Commit();
					Trace.WriteLine( "Transaction commited - message removed from queue" );
				}
			}
		}

		private void processMessage( Message message )
		{
			if ( message.Label != "OK" )
			{
				// simulate failure for poison messages
				throw new ApplicationException( "Simulated failure" );
			}
		}
	}
}
