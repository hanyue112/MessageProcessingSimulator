# MessageProcessingSimulator
Message Processing Simulator

Requirements
Functional Requirements
1.	Message generator
Message generator is simulating the source of the data and your application needs to be able to create N (1-26) message generators. 
And each message generator will produce the one type of messages on a random interval (5-10ms). 
And the number of message generators can be passed in as the argument.
Below is the message to be generator:
------------------------------------------------------
|SeqNum | Message Type | DatetimeCreated |
------------------------------------------------------
SeqNum: starting from 1 and increasing on each new message in each message generator
Message Type: using alphabet. For example, if there are 3 generators, 
first generator message type sets to ‘A’, second generator message type sets to ‘B’ and third generator message type sets to ‘C’
DatetimeCreated: the timestamp of message has been created

2.	Message consumer
a.	A single entry point consumer to receive all messages from the N message generators. 
    Implement the consumer that logs different message type messages into separated log files.
b.	Before logging the message, block the processing on a random interval (10-20ms) to simulate the message processing time.
c.	The messages of the same message type need to be processed in sequential order, 
    while different types of messages don’t have this requirement.

Non-functional Requirements
Try to maximum the throughput of the message processing in your implementation
