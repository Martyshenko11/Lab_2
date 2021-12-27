using System;
using System.IO;
namespace Lab_2
{
    class Program
    {
        static void Main(string[] args)
        {
            BTree t = new BTree(100);
            var rand = new Random();
            for (int i = 0; i < 10000; i++) t.insert(i, "v" + i);
            for (int i = 0; i < 15; i++)
            {
                int k = rand.Next(10000);
                Console.WriteLine("Search for the key " + k + " Value founded: " + t.Search(k) + " Number of passed nodes: " + BTreeNode.passed);
            }
        }
    }

    class BTreeNode
    {
        public struct data
        {
            public int key;
            public string value;

        }
        public data[] keys;
        public int degree; 
        public BTreeNode[] children; 
        public int num; 
        public bool isLeaf; 
        public static int passed;

        public BTreeNode(int deg, bool isLeaf)
        {

            this.degree = deg;
            this.isLeaf = isLeaf;
            this.keys = new data[2 * this.degree - 1]; 
            this.children = new BTreeNode[2 * this.degree];
            this.num = 0;

        }

        public int FindKey(int key)
        {

            int idx = 0;
          
            while (idx < num && keys[idx].key < key)
                ++idx;
            return idx;
        }
        public void Remove(int key)
        {

            int idx = FindKey(key);
            if (idx < num && keys[idx].key == key)
            { 
                if (isLeaf) 
                    RemoveFromLeaf(idx);
                else 
                    RemoveFromNonLeaf(idx);
            }
            else
            {
                if (isLeaf)
                { 
                    Console.WriteLine($"The key {key} is does not exist in the tree");
                    return;
                }

                bool flag = idx == num;

                if (children[idx].num < degree) 
                    Fill(idx);

                if (flag && idx > num)
                    children[idx - 1].Remove(key);
                else
                    children[idx].Remove(key);
            }
        }

        public void RemoveFromLeaf(int idx)
        {

            for (int i = idx + 1; i < num; ++i)
                keys[i - 1] = keys[i];
            num--;
        }

        public void RemoveFromNonLeaf(int idx)
        {

            int key = keys[idx].key;

            if (children[idx].num >= degree)
            {
                data pred = GetPas(idx);
                keys[idx].key = pred.key;
                keys[idx].value = pred.value;
                children[idx].Remove(pred.key);
            }


            else if (children[idx + 1].num >= degree)
            {
                data succ = GetSuc(idx);
                keys[idx].key = succ.key;
                keys[idx].value = succ.value;
                children[idx + 1].Remove(succ.key);
            }
            else
            {
                Merge(idx);
                children[idx].Remove(key);
            }
        }

        public data GetPas(int idx)
        {
            BTreeNode cur = children[idx];
            data res;
            while (!cur.isLeaf)
                cur = cur.children[cur.num];
            res.key = cur.keys[cur.num - 1].key;
            res.value = cur.keys[cur.num - 1].value;
            return res;
        }

        public data GetSuc(int idx)
        { 
            BTreeNode cur = children[idx + 1];
            data res;
            while (!cur.isLeaf)
                cur = cur.children[0];
            res.key = cur.keys[0].key;
            res.value = cur.keys[0].value;
            return res;
        }

        public void Fill(int idx)
        {
            if (idx != 0 && children[idx - 1].num >= degree)
                BorrowFromPrev(idx);
            else if (idx != num && children[idx + 1].num >= degree)
                borrowFromNext(idx);
            else
            {
                if (idx != num)
                    Merge(idx);
                else
                    Merge(idx - 1);
            }
        }

        public void BorrowFromPrev(int idx)
        {

            BTreeNode child = children[idx];
            BTreeNode sibling = children[idx - 1];


            for (int i = child.num - 1; i >= 0; --i) 
                child.keys[i + 1] = child.keys[i];

            if (!child.isLeaf)
            { 
                for (int i = child.num; i >= 0; --i)
                    child.children[i + 1] = child.children[i];
            }

            child.keys[0] = keys[idx - 1];
            if (!child.isLeaf) 
                child.children[0] = sibling.children[sibling.num];

            keys[idx - 1] = sibling.keys[sibling.num - 1];
            child.num += 1;
            sibling.num -= 1;
        }

        public void borrowFromNext(int idx)
        {

            BTreeNode child = children[idx];
            BTreeNode sibling = children[idx + 1];

            child.keys[child.num] = keys[idx];

            if (!child.isLeaf)
                child.children[child.num + 1] = sibling.children[0];

            keys[idx] = sibling.keys[0];

            for (int i = 1; i < sibling.num; ++i)
                sibling.keys[i - 1] = sibling.keys[i];

            if (!sibling.isLeaf)
            {
                for (int i = 1; i <= sibling.num; ++i)
                    sibling.children[i - 1] = sibling.children[i];
            }
            child.num += 1;
            sibling.num -= 1;
        }

        public void Merge(int idx)
        {

            BTreeNode child = children[idx];
            BTreeNode sibling = children[idx + 1];

            child.keys[degree - 1] = keys[idx];

            for (int i = 0; i < sibling.num; ++i)
                child.keys[i + degree] = sibling.keys[i];

            if (!child.isLeaf)
            {
                for (int i = 0; i <= sibling.num; ++i)
                    child.children[i + degree] = sibling.children[i];
            }

            for (int i = idx + 1; i < num; ++i)
                keys[i - 1] = keys[i];
            for (int i = idx + 2; i <= num; ++i)
                children[i - 1] = children[i];

            child.num += sibling.num + 1;
            num--;
        }


        public void insertNotFull(int key, string value)
        {

            int i = num - 1;

            if (isLeaf)
            { 
                while (i >= 0 && keys[i].key > key)
                {
                    keys[i + 1] = keys[i];
                    i--;
                }
                keys[i + 1].key = key;
                keys[i + 1].value = value;
                num = num + 1;
            }
            else
            {
                while (i >= 0 && keys[i].key > key)
                    i--;
                if (children[i + 1].num == 2 * degree - 1)
                { 
                    splitChild(i + 1, children[i + 1]);
                    if (keys[i + 1].key < key)
                        i++;
                }
                children[i + 1].insertNotFull(key, value);
            }
        }

        public void splitChild(int i, BTreeNode y)
        {
            BTreeNode z = new BTreeNode(y.degree, y.isLeaf);
            z.num = degree - 1;

            for (int j = 0; j < degree - 1; j++)
                z.keys[j] = y.keys[j + degree];
            if (!y.isLeaf)
            {
                for (int j = 0; j < degree; j++)
                    z.children[j] = y.children[j + degree];
            }
            y.num = degree - 1;

            for (int j = num; j >= i + 1; j--)
                children[j + 1] = children[j];
            children[i + 1] = z;

            for (int j = num - 1; j >= i; j--)
                keys[j + 1] = keys[j];
            keys[i] = y.keys[degree - 1];

            num = num + 1;
        }


        public void traverse()
        {
            int i;
            for (i = 0; i < num; i++)
            {
                if (!isLeaf)
                    children[i].traverse();
                Console.Write($" {keys[i].key}-{keys[i].value}");
            }

            if (!isLeaf)
            {
                children[i].traverse();
            }
        }

        public string tree_to_string()
        {
            string s = "";
            int i;
            for (i = 0; i < num; i++)
            {
                if (!isLeaf)
                    s += children[i].tree_to_string();
                s += keys[i].key + "@" + keys[i].value + "@";
            }

            if (!isLeaf)
            {
                s += children[i].tree_to_string();
            }
            return s;
        }
        public BTreeNode search(int key)
        {
            int i = 0;
            while (i < num && key > keys[i].key)
                i++;

            if (keys[i].key == key)
                return this;
            if (isLeaf)
                return null;
            return children[i].search(key);
        }
        public string Search(int key)
        {
            passed++;
            int left = -1;
            int right = num;
            while (left < right - 1)
            {
                int mid = (left + right) / 2;
                if (keys[mid].key < key)
                {
                    left = mid;
                }
                else
                {
                    right = mid;
                }
            }
            if (right < keys.Length)
                if (keys[right].key == key) return keys[right].value;
            if (isLeaf) return key + " Not founded";
            else return children[right].Search(key);
        }
    }

    class BTree
    {
        public BTreeNode root;
        int degree;

        public BTree(int deg)
        {
            this.root = null;
            this.degree = deg;
        }

        public void traverse()
        {
            if (root != null)
            {
                root.traverse();
            }
        }

        public BTreeNode search(int key)
        {
            BTreeNode.passed = 0;
            return root == null ? null : root.search(key);
        }

        public void insert(int key, string value)
        {

            if (root == null)
            {
                root = new BTreeNode(degree, true);
                root.keys[0].key = key;
                root.keys[0].value = value;
                root.num = 1;
            }
            else
            {
                if (root.num == 2 * degree - 1)
                {
                    BTreeNode s = new BTreeNode(degree, false);
                    s.children[0] = root;
                    s.splitChild(0, root);
                    int i = 0;
                    if (s.keys[0].key < key)
                        i++;
                    s.children[i].insertNotFull(key, value);

                    root = s;
                }
                else
                    root.insertNotFull(key, value);
            }
        }

        public void Remove(int key)
        {
            if (root == null)
            {
                Console.WriteLine("The tree is empty");
                return;
            }

            root.Remove(key);

            if (root.num == 0)
            { 
                if (root.isLeaf)
                    root = null;
                else
                    root = root.children[0];
            }
        }
        public string Search(int pkey)
        {
            BTreeNode.passed = 0;
            if (root != null) return root.Search(pkey);
            else return "Not founded";

        }
        public void redakt(int pkey, string val)
        {
            if (search(pkey) != null)
            {
                Remove(pkey);
                insert(pkey, val);
                Console.WriteLine($"Key changed {pkey}: value = {Search(pkey)}");
            }
            else Console.WriteLine($"key={pkey} not founded! ");
        }
        public void save()
        {
            if (root != null)
            {
                string fname = "data.txt";
                using (StreamWriter sw = new StreamWriter(fname, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(root.tree_to_string());
                }
            }
        }
        public void load()
        {
            string fname = "data.txt";
            string s = "";
            using (StreamReader sr = new StreamReader(fname))
            {
                s = sr.ReadToEnd();
                string[] dataS = s.Split("@");
                root = null;
                for (int i = 0; i < dataS.Length - 1; i += 2) insert(Int32.Parse(dataS[i]), dataS[i + 1]);
            }
        }
    }

}

