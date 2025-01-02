#include <cstdlib>
#include <iostream>
#include <sstream>
#include <string>

using namespace std;

int main() {
  string command = "git rev-list --all --count";
  FILE *pipe = popen(command.c_str(), "r");
  if (!pipe) {
    cerr << "Error: Could not open pipe for git command." << endl;
    return 1;
  }

  char buffer[128];
  string result;
  while (fgets(buffer, sizeof(buffer), pipe) != NULL) {
    result += buffer;
  }

  pclose(pipe);

  if (!result.empty() && result[result.size() - 1] == '\n') {
    result.erase(result.size() - 1);
  }

  int git_count = stoi(result);

  int patch = git_count % 9;
  int minor = git_count / 9;
  int major = minor / 9;

  cout << "v" << major << "." << minor << "." << patch << endl;

  return 0;
}