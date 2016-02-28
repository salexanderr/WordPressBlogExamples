import datetime
from pymongo import MongoClient
import re
import sys

def ImportLogentries(database,collection,filepath):
	"""
	Inserts the logentries from [filepath] into Mongodb running at the default (localhost:27017)
	in the specified [database] and [collection].
	"""
	# Get the logentries
	logentries = ParseLogfile(filepath)
	
	# Store the logentries in the database
	MongoClient()[database][collection].insert_many(logentries)

def ParseLogfile(filepath):
	"""
	Reads the logfile and returns the log entries. The format read is as follows:
	(type): "(user)" [(date)] 0ms "(function)" "(status)"
	"""
	
	try:
		# Open the specified log file for reading
		logfile = open(filepath, 'r')
		
		# Variable to store all of the entries
		entries = []
		
		# Regular expression compilation
		regex = re.compile(r"^(?P<type>[^\s]*):\s\"(?P<user>[^\"]*)\"\s\[(?P<date>[^\]]*)\]\s\d*ms\s\"(?P<function>[^\"]*)\"\s\"(?P<status>[^\"]*)\"")
			
		for line in logfile:
			print line
			# Match the line using the regular expression engine.
			matches = regex.match(line)
			
			# Store the matches.
			entry = {
				'date':datetime.datetime.strptime(matches.group("date"),'%Y-%m-%d %H:%M:%S'),
				'function':matches.group("function"),
				'status':matches.group("status"),
				'type':matches.group("type"),
				'user':matches.group("user"),
			}
			
			# Append the entry to the entries collection
			entries.append(entry)
	finally:		
		# Close the open file.
		logfile.close()
	
	return entries

# Main script entry
if __name__ == "__main__":
	database = sys.argv[1]
	collection = sys.argv[2]
	filename = sys.argv[3]
	ImportLogentries(database,collection,filename)