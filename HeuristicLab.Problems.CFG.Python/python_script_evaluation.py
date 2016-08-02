import json
import multiprocessing as mp
import sys
from queue import Empty
from types import ModuleType
import logging
logging.basicConfig(filename='python_log.txt', format='%(asctime)s:%(process)d:%(thread)d:%(message)s', level=logging.INFO) # set to DEBUG for debug info ;)

#import os
#logging.basicConfig(filename='python_log_' + str(os.getpid()) + '.txt', format='%(asctime)s:%(process)d:%(thread)d:%(message)s', level=logging.DEBUG) # set to DEBUG for debug info ;)


class Worker(mp.Process):
    def __init__(self, consume, produce):
        super(Worker, self).__init__()
        self.consume = consume
        self.produce = produce
        self.stop = mp.Value('b', False)

    def run(self):
        while True:
            exception = None
            self.stop.value = False
            script = self.consume.get()
            if script:
                help_globals = {'stop': self.stop}
                try:
                    exec(script, help_globals)
                except BaseException as e:
                    exc_type, exc_obj, exc_tb = sys.exc_info()
                    exception = '{} {} {}'.format(exc_type, exc_obj, e.args)
                if exception:
                    self.produce.put({'exception': exception})
                else:
                    self.produce.put({key: value for key, value in help_globals.items()
                                      if not callable(value) and             # cannot be a function
                                      not isinstance(value, ModuleType) and  # cannot be a module
                                      key not in ['__builtins__', 'stop']})  # cannot be built ins or synchronized objects
            else:
                break

    def stop_current(self):
        self.stop.value = True

if __name__ == '__main__':
    consume = mp.Queue()
    produce = mp.Queue()
    p = Worker(consume, produce)
    p.start()
    while True:
        try:
            message = input()
            logging.debug('Received input')
        except EOFError as err:
            # No data was read with input()
            # HeuristicLab is not running anymore
            # stop thread
            consume.put(None)
            if not p.join(5):
                p.terminate()
            break

        message_dict = json.loads(message)
        consume.put(message_dict['script'])
        try:
            results = produce.get(block=True, timeout=1.0)
        except Empty:
            results = None
        if not results:
            p.stop_current()
            produce.get()
            print(json.dumps({'exception': 'Timeout occurred.'}), flush=True)
            logging.debug('Sent output timeout')
        elif 'exception' in results:
            print(json.dumps(results), flush=True)
            logging.debug('Sent output exception')
        else:
            ret_message_dict = {}
            for v in message_dict['variables']:
                if v in results:
                    ret_message_dict[v] = results[v]
            print(json.dumps(ret_message_dict), flush=True)
            logging.debug('Sent output normal')
