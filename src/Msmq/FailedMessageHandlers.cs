using System;
using System.Diagnostics;
using System.Messaging;

namespace Cogin.Msmq
{
	public interface IFailedMessageHandler
	{	
		TransactionAction HandleFailedMessage( Message message, MessageQueueTransaction transaction );
	}

	public enum TransactionAction { ROLLBACK, COMMIT };

	public class DiscardMessageHandler : IFailedMessageHandler
	{
		public TransactionAction HandleFailedMessage( Message message, MessageQueueTransaction transaction )
		{
			Trace.WriteLine( "Message discarded" );
			return TransactionAction.COMMIT;
		}
	}

	public class AlwaysRollBackHandler : IFailedMessageHandler
	{
		public TransactionAction HandleFailedMessage( Message message, MessageQueueTransaction transaction )
		{
			return TransactionAction.ROLLBACK;
		}
	}

	public class SendToBackHandler : IFailedMessageHandler
	{
		const int MAX_RETRIES = 3;
		MessageQueue deadLetterQueue;
		
		public SendToBackHandler( MessageQueue deadLetterQueue )
		{
			this.deadLetterQueue = deadLetterQueue;
		}

		public TransactionAction HandleFailedMessage( Message message, MessageQueueTransaction transaction )
		{
			message.Priority = MessagePriority.Lowest;
			message.AppSpecific++;
			if ( message.AppSpecific > MAX_RETRIES )
			{
				Trace.WriteLine( "Sending to dead-letter queue" );
				deadLetterQueue.Send( message, transaction );
			} 
			else 
			{
				Trace.WriteLine( "Sending to back of queue" );
				message.DestinationQueue.Send( message, transaction );
			}
			return TransactionAction.COMMIT;
		}
	}

	public class RetrySendToDeadLetterQueueHandler : IFailedMessageHandler
	{
		static string lastMessageId = null;
		static int retries = 0; 
		const int MAX_RETRIES = 3;
		MessageQueue deadLetterQueue;
		
		public RetrySendToDeadLetterQueueHandler( MessageQueue deadLetterQueue )
		{
			this.deadLetterQueue = deadLetterQueue;
		}

		public TransactionAction HandleFailedMessage( Message message, MessageQueueTransaction transaction )
		{
			if ( message.Id != lastMessageId )
			{
				retries = 0;
				lastMessageId = message.Id;
			}
			retries++;
			if ( retries > MAX_RETRIES )
			{
				Trace.WriteLine( "Sending to dead-letter queue" );
				deadLetterQueue.Send( message, transaction );
				return TransactionAction.COMMIT;
			} else
			{
				Trace.WriteLine( "Returning message to queue for retry: " + retries );
				return TransactionAction.ROLLBACK;
			}
		}
	}

	public class SeparateRetryQueueHandler : IFailedMessageHandler
	{
		MessageQueue retryQueue;
		
		public SeparateRetryQueueHandler( MessageQueue retryQueue )
		{
			this.retryQueue = retryQueue;
		}

		public TransactionAction HandleFailedMessage( Message message, MessageQueueTransaction transaction )
		{
			Trace.WriteLine( "Sending to retry queue" );
			retryQueue.Send( message, transaction );
			return TransactionAction.COMMIT;
		}
	}
}
