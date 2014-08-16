﻿namespace AngleSharp.DOM
{
    using AngleSharp.DOM.Collections;
    using AngleSharp.DOM.Events;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a node in the generated tree.
    /// </summary>
    public class Node : INode, IHtmlObject
    {
        #region Fields

        readonly NodeType _type;
        readonly String _name;
        readonly NodeFlags _flags;
        readonly List<RegisteredEventListener> _listeners;

        Document _owner;
        String _baseUri;
        Node _parent;
        NodeList _children;

        #endregion

        #region ctor

        /// <summary>
        /// Constructs a new node.
        /// </summary>
        internal Node(String name, NodeType type = NodeType.Element, NodeFlags flags = NodeFlags.None)
        {
            _listeners = new List<RegisteredEventListener>();
            _name = name ?? String.Empty;
            _type = type;
            _children = new NodeList();
            _flags = flags;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a boolean value indicating whether the current Node 
        /// has child nodes or not.
        /// </summary>
        public Boolean HasChilds
        {
            get { return _children.Length != 0; }
        }

        /// <summary>
        /// Gets or sets the absolute base URI of a node or null if
        /// unable to obtain an absolute URI.
        /// </summary>
        public String BaseUri
        {
            get 
            {
                if (_baseUri != null)
                    return _baseUri;
                else if (_parent != null)
                    return _parent.BaseUri;
                else if (_owner != null)
                    return _owner.DocumentUri;

                return String.Empty;
            }
            set { _baseUri = value; }
        }

        /// <summary>
        /// Gets the node immediately preceding this node's parent's list of nodes, 
        /// null if the specified node is the first in that list.
        /// </summary>
        public INode PreviousSibling
        {
            get
            {
                if (_parent == null)
                    return null;

                var n = _parent._children.Length;

                for (int i = 1; i < n; i++)
                {
                    if (_parent._children[i] == this)
                        return _parent._children[i - 1];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the node immediately following this node's parent's list of nodes,
        /// or null if the current node is the last node in that list.
        /// </summary>
        public INode NextSibling
        {
            get
            {
                if (_parent == null)
                    return null;

                var n = _parent._children.Length - 1;

                for (int i = 0; i < n; i++)
                {
                    if (_parent._children[i] == this)
                        return _parent._children[i + 1];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the first child node of this node.
        /// </summary>
        public INode FirstChild
        {
            get { return _children.Length > 0 ? _children[0] : null; }
        }

        /// <summary>
        /// Gets the last child node of this node.
        /// </summary>
        public INode LastChild
        {
            get { return _children.Length > 0 ? _children[_children.Length - 1] : null; }
        }

        /// <summary>
        /// Gets the type of this node.
        /// </summary>
        public NodeType NodeType 
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets or sets the value of the current node.
        /// </summary>
        public virtual String NodeValue 
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// Gets or sets the text content of a node and its descendants.
        /// </summary>
        public virtual String TextContent
        {
            get { return null; }
            set { }
        }

        IDocument INode.Owner
        {
            get { return Owner; }
        }

        /// <summary>
        /// Gets the parent node.
        /// </summary>
        INode INode.Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Gets or sets the parent element of this node.
        /// </summary>
        public IElement ParentElement
        {
            get { return _parent as IElement; }
        }

        /// <summary>
        /// Gets the children of this node.
        /// </summary>
        INodeList INode.ChildNodes
        {
            get { return _children; }
        }

        /// <summary>
        /// Gets the tag name for this node.
        /// </summary>
        public String NodeName
        {
            get { return _name; }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets the flags of this node.
        /// </summary>
        internal NodeFlags Flags
        {
            get { return _flags; }
        }

        /// <summary>
        /// Gets or sets the children of this node.
        /// </summary>
        internal NodeList ChildNodes
        {
            get { return _children; }
            set { _children = value; }
        }

        /// <summary>
        /// Gets the parent node of this node, which is either an Element node,
        /// a Document node, or a DocumentFragment node.
        /// </summary>
        internal Node Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// Gets the owner document of the node.
        /// </summary>
        internal Document Owner
        {
            get { return _type != NodeType.Document ? _owner : null; }
            set
            {
                if (_owner == value)
                    return;

                _owner = value;

                for (int i = 0; i < _children.Length; i++)
                    _children[i].Owner = value;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Appends the given characters to the node.
        /// </summary>
        /// <param name="s">The characters to append.</param>
        internal void AppendText(String s)
        {
            var lastChild = LastChild as IText;

            if (lastChild == null)
                AddNode(new TextNode(s) { Owner = Owner });
            else
                lastChild.Append(s);
        }

        /// <summary>
        /// Inserts the given character in the node.
        /// </summary>
        /// <param name="index">The index where to insert.</param>
        /// <param name="s">The characters to append.</param>
        internal void InsertText(Int32 index, String s)
        {
            if (index > 0 && index <= _children.Length && _children[index - 1] is IText)
            {
                var node = (IText)_children[index - 1];
                node.Append(s);
            }
            else if (index >= 0 && index < _children.Length && _children[index] is IText)
            {
                var node = (IText)_children[index];
                node.Insert(0, s);
            }
            else
                InsertNode(index, new TextNode(s) { Owner = Owner });
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a child to the collection of children.
        /// </summary>
        /// <param name="child">The child to add.</param>
        /// <returns>The added child.</returns>
        public INode AppendChild(INode child)
        {
            return this.PreInsert(child, null);
        }

        /// <summary>
        /// Inserts a child to the collection of children at the specified index.
        /// </summary>
        /// <param name="index">The index where to insert.</param>
        /// <param name="child">The child to insert.</param>
        /// <returns>The inserted child.</returns>
        public INode InsertChild(Int32 index, INode child)
        {
            return this.PreInsert(child, _children[index]);
        }

        /// <summary>
        /// Inserts the specified node before a reference element as a child of the current node.
        /// </summary>
        /// <param name="newElement">The node to insert.</param>
        /// <param name="referenceElement">The node before which newElement is inserted. If
        /// referenceElement is null, newElement is inserted at the end of the list of child nodes.</param>
        /// <returns>The inserted node.</returns>
        public INode InsertBefore(INode newElement, INode referenceElement)
        {
            return this.PreInsert(newElement, referenceElement);
        }

        /// <summary>
        /// Replaces one child node of the specified element with another.
        /// </summary>
        /// <param name="newChild">The new node to replace oldChild. If it already exists in the DOM, it is first removed.</param>
        /// <param name="oldChild">The existing child to be replaced.</param>
        /// <returns>The replaced node. This is the same node as oldChild.</returns>
        public INode ReplaceChild(INode newChild, INode oldChild)
        {
            return this.ReplaceChild(newChild, oldChild, false);
        }

        /// <summary>
        /// Removes a child from the collection of children.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        /// <returns>The removed child.</returns>
        public INode RemoveChild(INode child)
        {
            return this.PreRemove(child);
        }

        /// <summary>
        /// Returns a duplicate of the node on which this method was called.
        /// </summary>
        /// <param name="deep">Optional value: true if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>The duplicate node.</returns>
        public virtual INode Clone(Boolean deep = true)
        {
            var node = new Node(_name, _type, _flags);
            CopyProperties(this, node, deep);
            return node;
        }

        /// <summary>
        /// Compares the position of the current node against another node in any other document.
        /// </summary>
        /// <param name="otherNode">The node that's being compared against.</param>
        /// <returns>The relationship that otherNode has with node, given in a bitmask.</returns>
        public DocumentPositions CompareDocumentPosition(INode otherNode)
        {
            if (this == otherNode)
                return DocumentPositions.Same;

            if (_owner != otherNode.Owner)
                return DocumentPositions.Disconnected | DocumentPositions.ImplementationSpecific | (otherNode.GetHashCode() > GetHashCode() ? DocumentPositions.Following : DocumentPositions.Preceding);
            else if (otherNode.IsAncestorOf(this))
                return DocumentPositions.Contains | DocumentPositions.Preceding;
            else if (otherNode.IsDescendentOf(this))
                return DocumentPositions.ContainedBy | DocumentPositions.Following;
            else if (otherNode.IsPreceding(this))
                return DocumentPositions.Preceding;

            return DocumentPositions.Following;
        }

        /// <summary>
        /// Indicates whether a node is a descendent of this node.
        /// </summary>
        /// <param name="otherNode">The node that's being compared against.</param>
        /// <returns>The return value is true if otherNode is a descendent of node, or node itself. Otherwise the return value is false.</returns>
        public Boolean Contains(INode otherNode)
        {
            return this.IsInclusiveAncestorOf(otherNode);
        }

        /// <summary>
        /// Puts the specified node and all of its subtree into a "normalized" form. In a normalized subtree, no text nodes in the
        /// subtree are empty and there are no adjacent text nodes.
        /// </summary>
        public void Normalize()
        {
            for (int i = 0; i < _children.Length; i++)
            {
                var text = _children[i] as IText;

                if (text != null)
                {
                    var length = text.Length;

                    if (length == 0)
                    {
                        RemoveChild(text, false);
                        i--;
                    }
                    else
                    {
                        var sb = Pool.NewStringBuilder();
                        var sibling = text;
                        var end = i;

                        while ((sibling = sibling.NextSibling as IText) != null)
                        {
                            sb.Append(sibling.Data);
                            end++;

                            //TODO
                            // For each range whose start node is sibling, add length to its start offset and set its start node to text. 
                            // For each range whose end node is sibling, add length to its end offset and set its end node to text. 
                            // For each range whose start node is sibling's parent and start offset is sibling's index, set its start node to text and its start offset to length. 
                            // For each range whose end node is sibling's parent and end offset is sibling's index, set its end node to node and its end offset to length. 

                            length += sibling.Length;
                        }

                        text.Replace(text.Length, 0, sb.ToPool());

                        for (int j = end; j > i; j--)
                            RemoveChild(_children[j], false);
                    }
                }
                else if (_children[i].HasChilds)
                    _children[i].Normalize();
            }
        }

        /// <summary>
        /// Takes a prefix and returns the namespaceURI associated with it on the given node if found (and null if not).
        /// Supplying null for the prefix will return the default namespace.
        /// </summary>
        /// <param name="prefix">The prefix to look for.</param>
        /// <returns>The namespace URI.</returns>
        public String LookupNamespaceUri(String prefix)
        {
            if (String.IsNullOrEmpty(prefix))
                prefix = null;

            return LocateNamespace(prefix);
        }

        /// <summary>
        /// Returns the prefix for a given namespaceURI if present, and null if not. When multiple prefixes are possible,
        /// the result is implementation-dependent.
        /// </summary>
        /// <param name="namespaceUri">The namespaceURI to lookup.</param>
        /// <returns>The prefix.</returns>
        public String LookupPrefix(String namespaceUri)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                return null;

            return LocatePrefix(namespaceUri);
        }

        /// <summary>
        /// Accepts a namespace URI as an argument and returns true if the namespace is the default namespace on the given node or false if not.
        /// </summary>
        /// <param name="namespaceUri">A string representing the namespace against which the element will be checked.</param>
        /// <returns>True if the given namespaceURI is the default namespace.</returns>
        public Boolean IsDefaultNamespace(String namespaceUri)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                namespaceUri = null;

            var defaultNamespace = LocateNamespace(null);
            return defaultNamespace == namespaceUri;
        }

        /// <summary>
        /// Tests whether two nodes are equal.
        /// </summary>
        /// <param name="otherNode">The node to compare equality with.</param>
        /// <returns>True if they are equal, otherwise false.</returns>
        public virtual Boolean IsEqualNode(INode otherNode)
        {
            if (BaseUri != otherNode.BaseUri)
                return false;

            if (NodeName != otherNode.NodeName)
                return false;

            if (ChildNodes.Length != otherNode.ChildNodes.Length)
                return false;

            for (int i = 0; i < _children.Length; i++)
            {
                if (!_children[i].IsEqualNode(otherNode.ChildNodes[i]))
                    return false;
            }

            return true;
        }

        #endregion

        #region Helpers

        protected virtual String LocateNamespace(String prefix)
        {
            if (_parent != null)
                return _parent.LocateNamespace(prefix);

            return null;
        }

        protected virtual String LocatePrefix(String namespaceUri)
        {
            if (_parent != null)
                return _parent.LocatePrefix(namespaceUri);

            return null;
        }

        /// <summary>
        /// Adopts the current node for the provided document.
        /// </summary>
        /// <param name="document">The new owner of the node.</param>
        internal void ChangeOwner(Document document)
        {
            var oldDocument = _owner;

            if (_parent != null)
                _parent.RemoveChild(this, false);

            Owner = document;
            NodeIsAdopted(oldDocument);
        }

        internal void InsertNode(Int32 index, Node node)
        {
            node.Parent = this;
            _children.Insert(index, node);
        }

        internal void AddNode(Node node)
        {
            node.Parent = this;
            _children.Add(node);
        }

        internal void RemoveNode(Int32 index, Node node)
        {
            node.Parent = null;
            _children.RemoveAt(index);
        }

        /// <summary>
        /// Replaces all nodes with the given node, if any.
        /// </summary>
        /// <param name="node">The node to insert, if any.</param>
        /// <param name="suppressObservers">If mutation observers should be surpressed.</param>
        internal void ReplaceAll(Node node, Boolean suppressObservers)
        {
            if (node != null)
                _owner.AdoptNode(node);

            var removedNodes = new NodeList(_children);
            var addedNodes = new NodeList();
            
            if (node != null)
            {
                if (node is IDocumentFragment)
                    addedNodes.AddRange(node._children);
                else
                    addedNodes.Add(node);
            }

            foreach (var child in removedNodes)
                RemoveChild(child, true);

            foreach (var child in addedNodes)
                InsertBefore(child, null, true);

            //TODO
            // Queue a mutation record of "childList" for parent with
            // addedNodes and removedNodes.
        }

        /// <summary>
        /// Inserts the specified node before a reference element as a child of the current node.
        /// </summary>
        /// <param name="newElement">The node to insert.</param>
        /// <param name="referenceElement">The node before which newElement is inserted. If
        /// referenceElement is null, newElement is inserted at the end of the list of child nodes.</param>
        /// <param name="suppressObservers">If mutation observers should be surpressed.</param>
        /// <returns>The inserted node.</returns>
        internal INode InsertBefore(INode newElement, INode referenceElement, Boolean suppressObservers)
        {
            var count = newElement is IDocumentFragment ? newElement.ChildNodes.Length : 1;

            if (referenceElement != null)
            {
                //TODO
                // For each range whose start node is parent and start offset is greater than child's index, increase its start offset by count. 
                // For each range whose end node is parent and end offset is greater than child's index, increase its end offset by count.
            }

            if (newElement is IDocument || newElement.Contains(this))
                throw new DomException(ErrorCode.HierarchyRequest);

            var n = _children.Index(referenceElement);

            if (n == -1)
                n = _children.Length;

            var fragment = newElement as DocumentFragment;

            if (fragment != null)
            {
                var start = n;

                while (fragment.HasChilds)
                {
                    var child = fragment.FirstChild as Node;
                    _children.Insert(n++, child);
                    fragment.RemoveChild(child, true);
                }

                while (start < n)
                    NodeIsInserted(_children[start++]);
            }
            else
            {
                InsertNode(n, newElement as Node);
                NodeIsInserted(newElement);
            }

            return newElement;
        }

        /// <summary>
        /// Removes a child from the collection of children.
        /// </summary>
        /// <param name="node">The child to remove.</param>
        /// <param name="suppressObservers">If mutation observers should be surpressed.</param>
        internal void RemoveChild(INode node, Boolean suppressObservers)
        {
            var index = _children.Index(node);

            //TODO
            // For each range whose start node is an inclusive descendant of node, set its start to (parent, index). 
            // For each range whose end node is an inclusive descendant of node, set its end to (parent, index). 
            // For each range whose start node is parent and start offset is greater than index, decrease its start offset by one. 
            // For each range whose end node is parent and end offset is greater than index, decrease its end offset by one. 

            var oldPreviousSibling = node.PreviousSibling;

            if (!suppressObservers)
            {
                //TODO
                // queue a mutation record of "childList" for parent with removedNodes a list solely containing node, nextSibling node's next sibling, and previousSibling oldPreviousSibling. 

                // For each ancestor ancestor of node, if ancestor has any registered observers whose options's subtree is true,
                // then for each such registered observer registered, append a transient registered observer whose observer and
                // options are identical to those of registered and source which is registered to node's list of registered observers. 
            }

            RemoveNode(index, node as Node);
            NodeIsRemoved(node, oldPreviousSibling);
        }

        /// <summary>
        /// Replaces one child node of the specified element with another.
        /// </summary>
        /// <param name="node">The new node to replace oldChild. If it already exists in the DOM, it is first removed.</param>
        /// <param name="child">The existing child to be replaced.</param>
        /// <param name="suppressObservers">If mutation observers should be surpressed.</param>
        /// <returns>The replaced node. This is the same node as oldChild.</returns>
        internal INode ReplaceChild(INode node, INode child, Boolean suppressObservers)
        {
            if (_type != NodeType.Document && _type != NodeType.DocumentFragment && _type != NodeType.Element)
                throw new DomException(ErrorCode.HierarchyRequest);
            else if (node.IsHostIncludingInclusiveAncestor(this))
                throw new DomException(ErrorCode.HierarchyRequest);
            else if (child.Parent != this)
                throw new DomException(ErrorCode.NotFound);

            if (node is IElement || node is ICharacterData || node is IDocumentFragment || node is IDocumentType)
            {
                if (_parent is IDocument)
                {
                    //6. If parent is a document, and any of the statements below, switched on node, are true, throw a "HierarchyRequestError". 
                    //    DocumentFragment node :
                    //            If node has more than one element child or has a Text node child. 
                    //            Otherwise, if node has one element child and either parent has an element child that is not child or a doctype is following child. 
                    //    element :
                    //            parent has an element child that is not child or a doctype is following child. 
                    //    doctype :
                    //            parent has a doctype child that is not child, or an element is preceding child. 
                }

                var referenceChild = child.NextSibling;

                if (referenceChild == node)
                    referenceChild = node.NextSibling;

                _owner.AdoptNode(node);
                RemoveChild(child, true);
                InsertBefore(node, referenceChild, true);
                var newNode = node as Node;
                var nodes = new NodeList();

                if (newNode != null)
                {
                    if (node is IDocumentFragment)
                        nodes.AddRange(newNode._children);
                    else
                        nodes.Add(newNode);
                }

                //TODO
                // Queue a mutation record of "childList" for target parent with addedNodes nodes, removedNodes a
                // list solely containing child, nextSibling reference child, and previousSibling child's previous sibling. 

                return child;
            }
            else
                throw new DomException(ErrorCode.HierarchyRequest);
        }

        void NodeIsAdopted(IDocument oldDocument)
        {
            //Run any adopting steps defined for node in other applicable specifications and pass node and oldDocument as parameters.
        }

        void NodeIsInserted(INode newNode)
        {
            //Specifications may define insertion steps for all or some nodes.
        }

        void NodeIsRemoved(INode removedNode, INode oldPreviousSibling)
        {
            //Specifications may define removing steps for all or some nodes.
        }

        /// <summary>
        /// Firing a simple event named e means that a trusted event with the name e,
        /// which does not bubble (except where otherwise stated) and is not cancelable
        /// (except where otherwise stated), and which uses the Event interface, must
        /// be created and dispatched at the given target.
        /// </summary>
        /// <param name="eventName">The name of the event to be fired.</param>
        /// <param name="bubble">Optional parameter to enable bubbling.</param>
        /// <param name="cancelable">Optional parameter to make it cancelable.</param>
        /// <returns>True if the element was cancelled, otherwise false.</returns>
        protected Boolean FireSimpleEvent(String eventName, Boolean bubble = false, Boolean cancelable = false)
        {
            var ev = new Event();
            ev.Init(eventName, bubble, cancelable);
            ev.IsTrusted = true;
            return ev.Dispatch(this);
        }

        /// <summary>
        /// Gets the hyperreference of the given URL -
        /// transforming the given (relative) URL to an absolute URL
        /// if required.
        /// </summary>
        /// <param name="url">The given URL.</param>
        /// <returns>The absolute URL.</returns>
        protected Url HyperRef(String url)
        {
            var baseUrl = new Url(BaseUri);
            return new Url(baseUrl, url ?? String.Empty);
        }

        /// <summary>
        /// Runs the mutation macro as defined in 5.2.2 Mutation methods
        /// of http://www.w3.org/TR/domcore/.
        /// </summary>
        /// <param name="nodes">The nodes array to add.</param>
        /// <returns>A (single) node.</returns>
        protected INode MutationMacro(INode[] nodes)
        {
            if (nodes.Length > 1)
            {
                var node = new DocumentFragment();

                for (int i = 0; i < nodes.Length; i++)
                    node.AppendChild(nodes[i]);

                return node;
            }

            return nodes[0];
        }

        /// <summary>
        /// Copies all (Node) properties of the source to the target.
        /// </summary>
        /// <param name="source">The source node.</param>
        /// <param name="target">The target node.</param>
        /// <param name="deep">Is a deep-copy required?</param>
        static protected void CopyProperties(Node source, Node target, Boolean deep)
        {
            target._owner = source._owner;
            target._baseUri = source._baseUri;

            if (deep)
            {
                for (int i = 0; i < source._children.Length; i++)
                    target._children.Add((Node)source._children[i].Clone(true));
            }
        }

        #endregion

        #region String representation

        /// <summary>
        /// Returns an HTML-code representation of the node.
        /// </summary>
        /// <returns>A string containing the HTML code.</returns>
        public virtual String ToHtml()
        {
            return TextContent;
        }

        #endregion

        #region Events

        /// <summary>
        /// Register an event handler of a specific event type on the Node.
        /// </summary>
        /// <param name="type">A string representing the event type to listen for.</param>
        /// <param name="callback">The listener parameter indicates the EventListener function to be added.</param>
        /// <param name="capture">True indicates that the user wishes to initiate capture. After initiating
        /// capture, all events of the specified type will be dispatched to the registered listener before being
        /// dispatched to any Node beneath it in the DOM tree. Events which are bubbling upward through the tree
        /// will not trigger a listener designated to use capture.</param>
        public void AddEventListener(String type, EventListener callback = null, Boolean capture = false)
        {
            if (callback == null)
                return;

            _listeners.Add(new RegisteredEventListener
            {
                Type = type,
                Callback = callback,
                IsCaptured = capture
            });
        }

        /// <summary>
        /// Removes an event listener from the Node.
        /// </summary>
        /// <param name="type">A string representing the event type being removed.</param>
        /// <param name="callback">The listener parameter indicates the EventListener function to be removed.</param>
        /// <param name="capture">Specifies whether the EventListener being removed was registered as a capturing listener or not.</param>
        public void RemoveEventListener(String type, EventListener callback = null, Boolean capture = false)
        {
            if (callback == null)
                return;

            _listeners.Remove(new RegisteredEventListener
            {
                Type = type,
                Callback = callback,
                IsCaptured = capture
            });
        }

        /// <summary>
        /// Calls the listener registered for the given event.
        /// </summary>
        /// <param name="ev">The event that asks for the listeners.</param>
        internal void CallEventListener(Event ev)
        {
            foreach (var listener in _listeners)
            {
                if (ev.Flags.HasFlag(EventFlags.StopImmediatePropagation))
                    break;

                if (listener.Type != ev.Type || listener.IsCaptured && ev.Phase == EventPhase.Bubbling || !listener.IsCaptured && ev.Phase == EventPhase.Capturing)
                    continue;

                listener.Callback(ev.CurrentTarget, ev);
            }
        }

        /// <summary>
        /// Dispatch an event to this Node.
        /// </summary>
        /// <param name="ev">The event to dispatch.</param>
        /// <returns>False if at least one of the event handlers, which handled this event called preventDefault(). Otherwise true.</returns>
        public Boolean Dispatch(IEvent ev)
        {
            var impl = ev as Event;

            if (impl == null || impl.Flags.HasFlag(EventFlags.Dispatch) || !impl.Flags.HasFlag(EventFlags.Initialized))
                throw new DomException(ErrorCode.InvalidState);

            impl.IsTrusted = false;
            return impl.Dispatch(this);
        }

        #endregion

        #region Event Listener Structure

        struct RegisteredEventListener
        {
            public String Type;
            public EventListener Callback;
            public Boolean IsCaptured;
        }

        #endregion
    }
}
